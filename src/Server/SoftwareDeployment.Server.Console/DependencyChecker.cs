using System.Diagnostics;
using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Console;

public sealed record CheckResult(string Name, bool Ok, string Detail);

/// <summary>Runs the "are we ready to run?" checks shown on the console's status tab.</summary>
public static class DependencyChecker
{
    public const string ServerExeName = "SoftwareDeployment.Server.exe";

    public static List<CheckResult> Run(string serverDir)
    {
        var results = new List<CheckResult>
        {
            CheckServerExe(serverDir),
            CheckAppSettings(serverDir),
            CheckDataDirWritable(serverDir),
            CheckAspNetRuntime(),
            CheckSecrets(serverDir),
            CheckDomainJoin(),
            CheckService(),
        };
        return results;
    }

    private static CheckResult CheckServerExe(string dir)
    {
        string path = Path.Combine(dir, ServerExeName);
        return File.Exists(path)
            ? new("Server-Programm", true, path)
            : new("Server-Programm", false, $"{ServerExeName} nicht in {dir} gefunden.");
    }

    private static CheckResult CheckAppSettings(string dir)
    {
        string path = Path.Combine(dir, "appsettings.json");
        if (!File.Exists(path))
            return new("Konfiguration", false, "appsettings.json fehlt.");
        string text = File.ReadAllText(path);
        bool https = text.Contains("https://", StringComparison.OrdinalIgnoreCase);
        return https
            ? new("Konfiguration (HTTPS)", true, "HTTPS-Endpunkt konfiguriert.")
            : new("Konfiguration (HTTPS)", false, "Kein HTTPS-Endpunkt in appsettings.json – unverschlüsselt nicht zulässig.");
    }

    private static CheckResult CheckDataDirWritable(string dir)
    {
        try
        {
            string data = Path.Combine(dir, "data");
            Directory.CreateDirectory(data);
            string probe = Path.Combine(data, ".write-test");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return new("Datenverzeichnis beschreibbar", true, data);
        }
        catch (Exception ex)
        {
            return new("Datenverzeichnis beschreibbar", false, ex.Message);
        }
    }

    private static CheckResult CheckAspNetRuntime()
    {
        try
        {
            var psi = new ProcessStartInfo("dotnet", "--list-runtimes")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi)!;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(10_000);

            bool hasAspNet10 = output
                .Split('\n')
                .Any(l => l.Contains("Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase)
                          && l.Contains(" 10.", StringComparison.Ordinal));

            return hasAspNet10
                ? new(".NET 10 ASP.NET Runtime", true, "ASP.NET Core 10 Runtime vorhanden.")
                : new(".NET 10 ASP.NET Runtime", false,
                      "ASP.NET Core 10 Runtime nicht gefunden (oder Server ist self-contained veröffentlicht).");
        }
        catch (Exception ex)
        {
            return new(".NET 10 ASP.NET Runtime", false, "dotnet nicht gefunden: " + ex.Message);
        }
    }

    private static CheckResult CheckSecrets(string dir)
    {
        string path = Path.Combine(dir, BootstrapSecrets.FileName);
        return File.Exists(path)
            ? new("Admin-Kennwort (Bootstrap)", true, "Bootstrap-Datei vorhanden.")
            : new("Admin-Kennwort (Bootstrap)", false,
                  "Noch kein Admin-Kennwort hinterlegt. Entweder hier setzen oder beim ersten Web-Aufruf.");
    }

    private static CheckResult CheckDomainJoin()
    {
        bool joined = !string.Equals(Environment.UserDomainName, Environment.MachineName,
                                     StringComparison.OrdinalIgnoreCase);
        return joined
            ? new("Domänenmitgliedschaft", true, $"Domäne: {Environment.UserDomainName}")
            : new("Domänenmitgliedschaft", false,
                  "Rechner scheint nicht domänengebunden – AD-Zuweisungen funktionieren dann nicht.");
    }

    private static CheckResult CheckService()
    {
        bool installed = ServiceManager.IsInstalled(DeploymentConstants.ServerServiceName);
        return installed
            ? new("Windows-Dienst", true, ServiceManager.GetStatusText(DeploymentConstants.ServerServiceName))
            : new("Windows-Dienst", false, "Dienst noch nicht installiert.");
    }
}
