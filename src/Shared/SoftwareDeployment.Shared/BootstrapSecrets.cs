namespace SoftwareDeployment.Shared;

/// <summary>
/// Written by the server console (<c>server-secrets.json</c>, placed next to the
/// server executable, ACL-restricted to the service account + admins) and read
/// by the server on startup. It only seeds the database on first run or when the
/// console explicitly resets the admin password, so the file can be deleted once
/// the server is configured.
/// </summary>
public sealed class BootstrapSecrets
{
    public const string FileName = "server-secrets.json";

    /// <summary>PBKDF2 hash produced by <see cref="Security.PasswordHasher"/>.</summary>
    public string? AdminPasswordHash { get; set; }

    /// <summary>
    /// Bumped by the console each time it writes the file. The server stores the
    /// last applied revision so it only re-seeds when the console changed it.
    /// </summary>
    public int Revision { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
