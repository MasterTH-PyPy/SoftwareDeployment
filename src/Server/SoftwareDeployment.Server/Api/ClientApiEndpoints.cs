using Microsoft.EntityFrameworkCore;
using SoftwareDeployment.Server.Data;
using SoftwareDeployment.Server.Services;
using SoftwareDeployment.Shared;
using SoftwareDeployment.Shared.Contracts;
using SoftwareDeployment.Shared.Security;

namespace SoftwareDeployment.Server.Api;

/// <summary>Minimal-API endpoints consumed by the client agent.</summary>
public static class ClientApiEndpoints
{
    private const int MaxMessageLength = 8000;

    public static void MapClientApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.ApiBase).WithTags("Client Agent");

        group.MapPost("/clients/register", RegisterAsync).AllowAnonymous();
        group.MapPost("/clients/poll", PollAsync);
        group.MapPost("/clients/report", ReportAsync);
        group.MapGet("/packages/{packageId:int}/versions/{version}/payload", DownloadAsync);
    }

    // ---- registration ----------------------------------------------------

    private static async Task<IResult> RegisterAsync(
        ClientRegistrationRequest req,
        DeploymentDbContext db,
        SettingsService settings,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.MachineName))
            return Results.BadRequest("MachineName is required.");

        // Gate 1: a valid enrollment key must be supplied at install time.
        if (!await settings.VerifyEnrollmentKeyAsync(req.EnrollmentKey, ct))
        {
            loggerFactory.CreateLogger("Enrollment").LogWarning(
                "Rejected registration from {Machine}: invalid enrollment key.", req.MachineName);
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        string machine = req.MachineName.Trim();
        string domain = (req.Domain ?? string.Empty).Trim();

        string apiKey = ApiKeys.Generate();
        string apiKeyHash = PasswordHasher.Hash(apiKey);

        ClientEntity? existing = await db.Clients
            .FirstOrDefaultAsync(c => c.MachineName == machine && c.Domain == domain, ct);

        SettingsEntity cfg = await settings.GetAsync(ct);

        ClientEntity client;
        if (existing is not null)
        {
            // Re-registration (agent reinstall): rotate the key, keep approval
            // unless it had been rejected. A still-pending client stays pending.
            client = existing;
            client.ApiKeyHash = apiKeyHash;
            client.OsVersion = req.OsVersion ?? string.Empty;
            client.AgentVersion = req.AgentVersion ?? string.Empty;
            client.LastSeenUtc = DateTime.UtcNow;
            if (client.Status == ClientApprovalStatus.Rejected)
                client.Status = ClientApprovalStatus.Pending;
        }
        else
        {
            client = new ClientEntity
            {
                Id = Guid.NewGuid(),
                MachineName = machine,
                Domain = domain,
                OsVersion = req.OsVersion ?? string.Empty,
                AgentVersion = req.AgentVersion ?? string.Empty,
                ApiKeyHash = apiKeyHash,
                Status = cfg.AutoApproveClients
                    ? ClientApprovalStatus.Approved
                    : ClientApprovalStatus.Pending,
                RegisteredAtUtc = DateTime.UtcNow,
                LastSeenUtc = DateTime.UtcNow,
            };
            db.Clients.Add(client);
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new ClientRegistrationResponse
        {
            ClientId = client.Id,
            ApiKey = apiKey, // only time the plaintext key leaves the server
            Status = client.Status,
        });
    }

    // ---- poll ------------------------------------------------------------

    private static async Task<IResult> PollAsync(
        HttpContext http,
        PollRequest req,
        DeploymentDbContext db,
        DeploymentService deployment,
        CancellationToken ct)
    {
        (ClientEntity? client, IResult? failure) = await AuthenticateAsync(http, db, ct);
        if (failure is not null) return failure;

        PollResponse response = await deployment.BuildPollResponseAsync(client!, req, ct);
        return Results.Ok(response);
    }

    // ---- report ----------------------------------------------------------

    private static async Task<IResult> ReportAsync(
        HttpContext http,
        InstallReportRequest req,
        DeploymentDbContext db,
        CancellationToken ct)
    {
        (ClientEntity? client, IResult? failure) = await AuthenticateAsync(http, db, ct);
        if (failure is not null) return failure;

        DateTime now = DateTime.UtcNow;

        foreach (InstallResultDto r in req.Results)
        {
            string? message = r.Message is { Length: > MaxMessageLength }
                ? r.Message[..MaxMessageLength]
                : r.Message;

            // History log line.
            string packageName = await db.Packages
                .Where(p => p.Id == r.PackageId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(ct) ?? $"#{r.PackageId}";

            db.InstallLogs.Add(new InstallLogEntity
            {
                ClientId = client!.Id,
                PackageId = r.PackageId,
                PackageName = packageName,
                Version = r.Version,
                Status = r.Status,
                ExitCode = r.ExitCode,
                Message = message,
                StartedAtUtc = r.StartedAtUtc.UtcDateTime,
                FinishedAtUtc = r.FinishedAtUtc.UtcDateTime,
                ReportedAtUtc = now,
            });

            // Current state (upsert).
            ClientPackageStateEntity? state = await db.ClientPackageStates
                .FirstOrDefaultAsync(s => s.ClientId == client.Id && s.PackageId == r.PackageId, ct);

            if (state is null)
            {
                state = new ClientPackageStateEntity
                {
                    ClientId = client.Id,
                    PackageId = r.PackageId,
                };
                db.ClientPackageStates.Add(state);
            }

            // Only advance the recorded installed version on success.
            if (r.Status == InstallResultStatus.Success || r.Status == InstallResultStatus.Skipped)
                state.InstalledVersion = r.Version;

            state.Status = r.Status;
            state.UpdatedAtUtc = now;
        }

        client!.LastSeenUtc = now;
        await db.SaveChangesAsync(ct);
        return Results.Ok();
    }

    // ---- download --------------------------------------------------------

    private static async Task<IResult> DownloadAsync(
        int packageId,
        string version,
        HttpContext http,
        DeploymentDbContext db,
        IDirectoryService directory,
        PackageStorage storage,
        CancellationToken ct)
    {
        (ClientEntity? client, IResult? failure) = await AuthenticateAsync(http, db, ct);
        if (failure is not null) return failure;

        if (client!.Status != ClientApprovalStatus.Approved)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        PackageVersionEntity? pv = await db.PackageVersions
            .FirstOrDefaultAsync(v => v.PackageId == packageId && v.Version == version, ct);

        if (pv is null || !storage.Exists(pv.PayloadStoredName))
            return Results.NotFound();

        // Confirm the package is actually assigned to this client before serving it
        // (AD-based or direct client assignment).
        IReadOnlyCollection<string> sids =
            directory.GetEffectiveSids(client.MachineName, client.LastKnownUser);
        var sidList = sids.ToList();
        string clientIdKey = client.Id.ToString();

        bool assigned = await db.Assignments.AnyAsync(a => a.PackageId == packageId
            && ((a.TargetType == AssignmentTargetType.Client && a.TargetSid == clientIdKey)
                || (a.TargetType != AssignmentTargetType.Client && sidList.Contains(a.TargetSid))), ct);

        if (!assigned)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        Stream stream = storage.OpenRead(pv.PayloadStoredName);
        string fileName = string.IsNullOrWhiteSpace(pv.PayloadFileName)
            ? $"package-{packageId}-{version}.bin"
            : pv.PayloadFileName;

        return Results.File(stream, "application/octet-stream", fileName, enableRangeProcessing: true);
    }

    // ---- shared auth -----------------------------------------------------

    private static async Task<(ClientEntity? client, IResult? failure)> AuthenticateAsync(
        HttpContext http, DeploymentDbContext db, CancellationToken ct)
    {
        string? idRaw = http.Request.Headers[ApiRoutes.ClientIdHeader];
        string? apiKey = http.Request.Headers[ApiRoutes.ApiKeyHeader];

        if (string.IsNullOrWhiteSpace(idRaw) || string.IsNullOrWhiteSpace(apiKey)
            || !Guid.TryParse(idRaw, out Guid id))
        {
            return (null, Results.StatusCode(StatusCodes.Status401Unauthorized));
        }

        ClientEntity? client = await db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (client is null || !PasswordHasher.Verify(apiKey, client.ApiKeyHash))
            return (null, Results.StatusCode(StatusCodes.Status401Unauthorized));

        return (client, null);
    }
}
