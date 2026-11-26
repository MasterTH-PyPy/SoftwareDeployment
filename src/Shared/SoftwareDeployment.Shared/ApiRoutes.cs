namespace SoftwareDeployment.Shared;

/// <summary>
/// Canonical route templates for the client-facing REST API. Kept in the
/// shared project so server and client agent never drift apart.
/// </summary>
public static class ApiRoutes
{
    public const string ApiBase = "/api/v1";

    /// <summary>POST — a client registers itself; returns id + api key, state Pending.</summary>
    public const string Register = ApiBase + "/clients/register";

    /// <summary>POST — the recurring poll; client sends its state, server returns due tasks.</summary>
    public const string Poll = ApiBase + "/clients/poll";

    /// <summary>POST — client reports install results.</summary>
    public const string Report = ApiBase + "/clients/report";

    /// <summary>GET — download a package payload. Placeholders: {packageId}/{version}.</summary>
    public const string DownloadTemplate = ApiBase + "/packages/{packageId}/versions/{version}/payload";

    public static string Download(int packageId, string version)
        => $"{ApiBase}/packages/{packageId}/versions/{Uri.EscapeDataString(version)}/payload";

    /// <summary>Header carrying the per-client api key on every authenticated call.</summary>
    public const string ClientIdHeader = "X-Client-Id";
    public const string ApiKeyHeader = "X-Api-Key";
}
