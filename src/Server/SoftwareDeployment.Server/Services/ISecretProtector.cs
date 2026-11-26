using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace SoftwareDeployment.Server.Services;

/// <summary>
/// At-rest protection for the few secrets that must be stored recoverably (the
/// enrollment key, which the admin has to read back in the UI). On Windows this
/// is DPAPI (LocalMachine scope); nothing is ever persisted in clear text.
/// </summary>
public interface ISecretProtector
{
    /// <summary>True when real OS-backed encryption is in effect.</summary>
    bool IsHardwareBacked { get; }

    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}

/// <summary>DPAPI-based protector. LocalMachine scope so the service account can read it.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsSecretProtector : ISecretProtector
{
    // Extra entropy so another LocalMachine process can't trivially unprotect our blobs.
    private static readonly byte[] Entropy =
        Encoding.UTF8.GetBytes("SoftwareDeployment.Server.v1");

    public bool IsHardwareBacked => true;

    public string Protect(string plaintext)
    {
        byte[] data = Encoding.UTF8.GetBytes(plaintext);
        byte[] blob = ProtectedData.Protect(data, Entropy, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(blob);
    }

    public string Unprotect(string protectedValue)
    {
        byte[] blob = Convert.FromBase64String(protectedValue);
        byte[] data = ProtectedData.Unprotect(blob, Entropy, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(data);
    }
}

/// <summary>
/// Fallback for non-Windows dev/CI. It does NOT provide real encryption, so it
/// logs a loud warning and tags stored values; production always runs on Windows.
/// </summary>
public sealed class PassthroughSecretProtector : ISecretProtector
{
    private const string Prefix = "plain:";

    public PassthroughSecretProtector(ILogger<PassthroughSecretProtector> logger)
        => logger.LogWarning(
            "Secret protection is NOT OS-backed on this platform. Run the server on " +
            "domain-joined Windows for DPAPI-protected secrets at rest.");

    public bool IsHardwareBacked => false;

    public string Protect(string plaintext)
        => Prefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(plaintext));

    public string Unprotect(string protectedValue)
    {
        string body = protectedValue.StartsWith(Prefix, StringComparison.Ordinal)
            ? protectedValue[Prefix.Length..]
            : protectedValue;
        return Encoding.UTF8.GetString(Convert.FromBase64String(body));
    }
}
