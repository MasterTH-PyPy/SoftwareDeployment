using System.Diagnostics;
using System.ServiceProcess;

namespace SoftwareDeployment.Server.Console;

/// <summary>Thin wrapper over sc.exe + ServiceController for installing, configuring
/// and controlling the deployment server Windows service.</summary>
public static class ServiceManager
{
    public static bool IsInstalled(string serviceName)
        => ServiceController.GetServices().Any(s =>
            string.Equals(s.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));

    public static string GetStatusText(string serviceName)
    {
        try
        {
            if (!IsInstalled(serviceName)) return "Nicht installiert";
            using var sc = new ServiceController(serviceName);
            return sc.Status switch
            {
                ServiceControllerStatus.Running => "Läuft",
                ServiceControllerStatus.Stopped => "Gestoppt",
                ServiceControllerStatus.StartPending => "Startet…",
                ServiceControllerStatus.StopPending => "Stoppt…",
                ServiceControllerStatus.Paused => "Pausiert",
                _ => sc.Status.ToString(),
            };
        }
        catch (Exception ex)
        {
            return "Fehler: " + ex.Message;
        }
    }

    public static (bool ok, string log) Install(
        string serviceName, string displayName, string exePath,
        string account, string? password, string? description)
    {
        // sc create <name> binPath= "<exe>" start= auto obj= <acct> [password= <pw>] DisplayName= "<dn>"
        var args = new List<string>
        {
            "create", serviceName,
            "binPath=", QuoteIfNeeded(exePath),
            "start=", "auto",
            "obj=", account,
        };
        if (!string.IsNullOrEmpty(password))
        {
            args.Add("password=");
            args.Add(password);
        }
        else if (!account.Equals("LocalSystem", StringComparison.OrdinalIgnoreCase)
                 && !account.StartsWith("NT ", StringComparison.OrdinalIgnoreCase))
        {
            // gMSA: explicit empty password.
            args.Add("password=");
            args.Add("");
        }
        args.Add("DisplayName=");
        args.Add(displayName);

        (bool ok, string log) = RunSc(args);
        if (ok && !string.IsNullOrWhiteSpace(description))
        {
            RunSc(["description", serviceName, description]);
        }
        return (ok, log);
    }

    public static (bool ok, string log) SetAccount(string serviceName, string account, string? password)
    {
        var args = new List<string> { "config", serviceName, "obj=", account, "password=", password ?? "" };
        return RunSc(args);
    }

    public static (bool ok, string log) Uninstall(string serviceName)
    {
        TryStop(serviceName);
        return RunSc(["delete", serviceName]);
    }

    public static (bool ok, string log) Start(string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status != ServiceControllerStatus.Running)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            }
            return (true, "Dienst gestartet.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public static (bool ok, string log) Stop(string serviceName)
    {
        try
        {
            TryStop(serviceName);
            return (true, "Dienst gestoppt.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static void TryStop(string serviceName)
    {
        if (!IsInstalled(serviceName)) return;
        using var sc = new ServiceController(serviceName);
        if (sc.Status is ServiceControllerStatus.Running or ServiceControllerStatus.Paused)
        {
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
        }
    }

    private static (bool ok, string log) RunSc(IEnumerable<string> arguments)
    {
        var psi = new ProcessStartInfo("sc.exe")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (string a in arguments) psi.ArgumentList.Add(a);

        try
        {
            using var p = Process.Start(psi)!;
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit(30_000);
            string log = (stdout + stderr).Trim();
            return (p.ExitCode == 0, string.IsNullOrEmpty(log) ? $"sc exit {p.ExitCode}" : log);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static string QuoteIfNeeded(string path)
        => path.Contains(' ') && !path.StartsWith('"') ? $"\"{path}\"" : path;
}
