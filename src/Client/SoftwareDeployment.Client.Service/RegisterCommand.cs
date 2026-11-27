using SoftwareDeployment.Client.Core;
using SoftwareDeployment.Shared.Contracts;

namespace SoftwareDeployment.Client.Service;

/// <summary>
/// Headless registration used by the installer:
///   SoftwareDeployment.Agent.exe register --server &lt;https-url&gt; --key &lt;enrollment-key&gt;
/// Performs the HTTPS registration and stores the DPAPI-protected credentials, so
/// the enrollment key is never written to a config file in clear text.
/// </summary>
public static class RegisterCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        string? server = GetArg(args, "--server");
        string? key = GetArg(args, "--key");

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(key))
        {
            Console.Error.WriteLine("Verwendung: register --server <https-url> --key <enrollment-key>");
            return 2;
        }
        if (!server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Die Server-URL muss mit https:// beginnen.");
            return 2;
        }

        try
        {
            var request = new ClientRegistrationRequest
            {
                MachineName = ClientInfo.MachineName,
                Domain = ClientInfo.Domain,
                OsVersion = ClientInfo.OsVersion,
                AgentVersion = ClientInfo.AgentVersion,
                EnrollmentKey = key,
            };

            using var api = new DeploymentApiClient(server);
            ClientRegistrationResponse resp = await api.RegisterAsync(request);

            AgentConfigStore.SaveRegistration(server, resp.ClientId, resp.ApiKey);
            AgentLog.Info($"Registriert bei {server} (Status {resp.Status}).");
            Console.WriteLine($"Registrierung erfolgreich. Status: {resp.Status}.");
            return 0;
        }
        catch (Exception ex)
        {
            AgentLog.Error("Registrierung (Installer) fehlgeschlagen: " + ex.Message);
            Console.Error.WriteLine("Registrierung fehlgeschlagen: " + ex.Message);
            return 1;
        }
    }

    private static string? GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }
}
