using System.Text.Json;
using SoftwareDeployment.Shared;
using SoftwareDeployment.Shared.Security;

namespace SoftwareDeployment.Server.Console;

/// <summary>
/// Writes the admin-password bootstrap file read by the server on startup. Only
/// a PBKDF2 hash is stored — never the plaintext password. The revision counter
/// is bumped on each write so the server applies the change exactly once.
/// </summary>
public static class SecretsWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string SetAdminPassword(string serverDir, string password)
    {
        string path = Path.Combine(serverDir, BootstrapSecrets.FileName);

        int revision = 0;
        if (File.Exists(path))
        {
            try
            {
                BootstrapSecrets? existing =
                    JsonSerializer.Deserialize<BootstrapSecrets>(File.ReadAllText(path));
                revision = existing?.Revision ?? 0;
            }
            catch { /* corrupt file -> start fresh */ }
        }

        var secrets = new BootstrapSecrets
        {
            AdminPasswordHash = PasswordHasher.Hash(password),
            Revision = revision + 1,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };

        File.WriteAllText(path, JsonSerializer.Serialize(secrets, JsonOptions));
        return path;
    }
}
