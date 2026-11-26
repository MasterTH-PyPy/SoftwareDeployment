using Microsoft.EntityFrameworkCore;
using SoftwareDeployment.Server.Data;
using SoftwareDeployment.Shared;
using SoftwareDeployment.Shared.Contracts;

namespace SoftwareDeployment.Server.Services;

/// <summary>
/// Turns a client poll into the list of packages it still needs. This is the
/// server-side successor to the old GPO script's computerinclude/groupinclude
/// logic: assignments are matched against the SIDs that apply to the polling
/// machine and user, and a package is handed out only when the client's
/// reported installed version differs from the current one.
/// </summary>
public sealed class DeploymentService(
    DeploymentDbContext db,
    IDirectoryService directory,
    SettingsService settings,
    ILogger<DeploymentService> logger)
{
    public async Task<PollResponse> BuildPollResponseAsync(
        ClientEntity client, PollRequest req, CancellationToken ct = default)
    {
        SettingsEntity cfg = await settings.GetAsync(ct);

        var response = new PollResponse
        {
            Status = client.Status,
            PollIntervalSeconds = cfg.PollIntervalSeconds,
        };

        // Refresh the "last seen" fields on every contact.
        client.LastSeenUtc = DateTime.UtcNow;
        client.LastKnownUser = req.CurrentUser;
        if (!string.IsNullOrWhiteSpace(req.AgentVersion))
            client.AgentVersion = req.AgentVersion;

        if (client.Status != ClientApprovalStatus.Approved)
        {
            await db.SaveChangesAsync(ct);
            return response; // Pending/Rejected clients get no work.
        }

        IReadOnlyCollection<string> effectiveSids =
            directory.GetEffectiveSids(client.MachineName, req.CurrentUser);

        if (effectiveSids.Count == 0 && !directory.IsAvailable)
            logger.LogWarning(
                "Directory unavailable; client {Machine} only receives direct (client-targeted) assignments.",
                client.MachineName);

        var sidList = effectiveSids.ToList();
        string clientIdKey = client.Id.ToString();

        // AD-based assignments (computer/group/user via SID) AND direct client
        // assignments (no AD needed). The direct path makes local/no-domain
        // testing and pilot rollouts work.
        List<int> assignedPackageIds = await db.Assignments
            .Where(a => (a.TargetType == AssignmentTargetType.Client && a.TargetSid == clientIdKey)
                        || (a.TargetType != AssignmentTargetType.Client && sidList.Contains(a.TargetSid)))
            .Select(a => a.PackageId)
            .Distinct()
            .ToListAsync(ct);

        if (assignedPackageIds.Count == 0)
        {
            await db.SaveChangesAsync(ct);
            return response;
        }

        List<PackageVersionEntity> current = await db.PackageVersions
            .Include(v => v.Package)
            .Where(v => v.IsCurrent
                        && v.Package.IsEnabled
                        && assignedPackageIds.Contains(v.PackageId))
            .ToListAsync(ct);

        Dictionary<int, string> installed = req.InstalledPackages
            .GroupBy(p => p.PackageId)
            .ToDictionary(g => g.Key, g => g.Last().Version);

        foreach (PackageVersionEntity v in current)
        {
            if (installed.TryGetValue(v.PackageId, out string? have)
                && VersionMatches(have, v.Version))
            {
                continue; // already up to date -> skip, exactly like the old script
            }

            response.Tasks.Add(new DeploymentTaskDto
            {
                PackageId = v.PackageId,
                PackageName = v.Package.Name,
                Version = v.Version,
                InstallerType = v.InstallerType,
                CommandFileName = v.CommandFileName,
                Arguments = v.Arguments,
                PayloadFileName = v.PayloadFileName,
                PayloadSha256 = v.PayloadSha256,
                PayloadSize = v.PayloadSize,
                SuccessExitCodes = v.SuccessExitCodes,
                PayloadIsArchive = v.PayloadIsArchive,
            });
        }

        await db.SaveChangesAsync(ct);
        return response;
    }

    private static bool VersionMatches(string a, string b)
        => string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
}
