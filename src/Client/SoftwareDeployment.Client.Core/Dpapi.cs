using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace SoftwareDeployment.Client.Core;

/// <summary>
/// DPAPI (LocalMachine) protection so the GUI (elevated) can store the API key and
/// the service account can read it back on the same machine. Never stored in clear.
/// </summary>
[SupportedOSPlatform("windows")]
public static class Dpapi
{
    private static readonly byte[] Entropy =
        Encoding.UTF8.GetBytes("SonOG.DeploymentAgent.v1");

    public static string Protect(string plaintext)
    {
        byte[] blob = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(plaintext), Entropy, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(blob);
    }

    public static string Unprotect(string protectedBase64)
    {
        byte[] data = ProtectedData.Unprotect(
            Convert.FromBase64String(protectedBase64), Entropy, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(data);
    }
}
