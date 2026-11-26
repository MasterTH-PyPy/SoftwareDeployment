using System.Security.Cryptography;

namespace SoftwareDeployment.Server.Services;

/// <summary>Generates the opaque per-client API keys handed out at registration.</summary>
public static class ApiKeys
{
    /// <summary>A 256-bit URL-safe random secret.</summary>
    public static string Generate()
    {
        byte[] raw = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(raw)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
