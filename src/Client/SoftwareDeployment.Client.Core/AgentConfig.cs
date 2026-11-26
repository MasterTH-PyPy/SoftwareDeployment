using System.Runtime.Versioning;
using System.Text.Json;

namespace SoftwareDeployment.Client.Core;

/// <summary>Persisted agent configuration. The API key is stored DPAPI-protected.</summary>
public sealed class AgentConfig
{
    public string ServerBaseUrl { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }

    /// <summary>DPAPI-protected API key (never the plaintext).</summary>
    public string? ApiKeyProtected { get; set; }

    public bool IsRegistered => ClientId is not null && !string.IsNullOrEmpty(ApiKeyProtected);
}

[SupportedOSPlatform("windows")]
public static class AgentConfigStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static AgentConfig Load()
    {
        try
        {
            if (File.Exists(AgentPaths.ConfigFile))
                return JsonSerializer.Deserialize<AgentConfig>(File.ReadAllText(AgentPaths.ConfigFile))
                       ?? new AgentConfig();
        }
        catch { /* fall through to empty */ }
        return new AgentConfig();
    }

    public static void Save(AgentConfig config)
    {
        AgentPaths.EnsureCreated();
        File.WriteAllText(AgentPaths.ConfigFile, JsonSerializer.Serialize(config, Json));
    }

    /// <summary>Store server URL + credentials after a successful registration.</summary>
    public static void SaveRegistration(string serverBaseUrl, Guid clientId, string apiKeyPlain)
    {
        var cfg = Load();
        cfg.ServerBaseUrl = serverBaseUrl.TrimEnd('/');
        cfg.ClientId = clientId;
        cfg.ApiKeyProtected = Dpapi.Protect(apiKeyPlain);
        Save(cfg);
    }

    public static string? GetApiKey(AgentConfig cfg)
        => string.IsNullOrEmpty(cfg.ApiKeyProtected) ? null : Dpapi.Unprotect(cfg.ApiKeyProtected);
}
