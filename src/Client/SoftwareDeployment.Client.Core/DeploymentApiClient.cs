using System.Net;
using System.Net.Http.Json;
using SoftwareDeployment.Shared;
using SoftwareDeployment.Shared.Contracts;

namespace SoftwareDeployment.Client.Core;

/// <summary>
/// HTTPS client for the deployment server API. TLS is validated by the OS trust
/// store (no unencrypted transport, no certificate bypass).
/// </summary>
public sealed class DeploymentApiClient : IDisposable
{
    private readonly HttpClient _http;

    public DeploymentApiClient(string baseUrl, Guid? clientId = null, string? apiKey = null)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromMinutes(30), // large payload downloads
        };
        if (clientId is not null)
            _http.DefaultRequestHeaders.Add(ApiRoutes.ClientIdHeader, clientId.Value.ToString());
        if (!string.IsNullOrEmpty(apiKey))
            _http.DefaultRequestHeaders.Add(ApiRoutes.ApiKeyHeader, apiKey);
    }

    public async Task<ClientRegistrationResponse> RegisterAsync(
        ClientRegistrationRequest request, CancellationToken ct = default)
    {
        using HttpResponseMessage resp = await _http.PostAsJsonAsync(ApiRoutes.Register, request, ct);

        if (resp.StatusCode == HttpStatusCode.Forbidden)
            throw new InvalidOperationException("Registrierung abgelehnt – Enrollment-Key ungültig.");
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<ClientRegistrationResponse>(ct)
               ?? throw new InvalidOperationException("Leere Antwort auf die Registrierung.");
    }

    public async Task<PollResponse> PollAsync(PollRequest request, CancellationToken ct = default)
    {
        using HttpResponseMessage resp = await _http.PostAsJsonAsync(ApiRoutes.Poll, request, ct);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException("Nicht autorisiert – Registrierung ungültig (ggf. neu registrieren).");
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<PollResponse>(ct)
               ?? new PollResponse();
    }

    public async Task ReportAsync(InstallReportRequest request, CancellationToken ct = default)
    {
        using HttpResponseMessage resp = await _http.PostAsJsonAsync(ApiRoutes.Report, request, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DownloadAsync(int packageId, string version, string destPath, CancellationToken ct = default)
    {
        string url = ApiRoutes.Download(packageId, version);
        using HttpResponseMessage resp = await _http.GetAsync(
            url, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        await using Stream src = await resp.Content.ReadAsStreamAsync(ct);
        await using var dst = new FileStream(
            destPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20, useAsync: true);
        await src.CopyToAsync(dst, ct);
    }

    public void Dispose() => _http.Dispose();
}
