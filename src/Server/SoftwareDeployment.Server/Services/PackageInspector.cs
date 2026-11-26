using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Services;

/// <summary>
/// Helpers for the package upload flow: detect installer type from the file name,
/// suggest silent-install defaults, read the version out of an MSI/EXE, and run
/// static ("dry") validation of a payload without executing anything.
/// </summary>
public static class PackageInspector
{
    public static bool IsArchive(string fileName)
        => Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase);

    public static InstallerType? DetectType(string fileName)
        => Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".msi" => InstallerType.Msi,
            ".exe" => InstallerType.Exe,
            ".bat" or ".cmd" => InstallerType.Bat,
            ".ps1" => InstallerType.PowerShell,
            _ => null, // e.g. .zip -> inner type unknown, user picks
        };

    /// <summary>Common silent-install switches per installer type.</summary>
    public static string DefaultArguments(InstallerType type) => type switch
    {
        InstallerType.Msi => "/qn /norestart",
        InstallerType.Exe => "/S",
        InstallerType.PowerShell => "-ExecutionPolicy Bypass -NoProfile",
        _ => "",
    };

    /// <summary>Best-effort version read; null if it cannot be determined.</summary>
    public static string? ReadVersion(string filePath, InstallerType type)
    {
        try
        {
            return type switch
            {
                InstallerType.Msi when OperatingSystem.IsWindows() => ReadMsiProductVersion(filePath),
                InstallerType.Exe => ReadExeVersion(filePath),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadExeVersion(string filePath)
    {
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath);
        string? v = !string.IsNullOrWhiteSpace(fvi.ProductVersion) ? fvi.ProductVersion
                  : fvi.FileVersion;
        return string.IsNullOrWhiteSpace(v) ? null : v!.Trim();
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadMsiProductVersion(string filePath)
    {
        Type? t = Type.GetTypeFromProgID("WindowsInstaller.Installer");
        if (t is null) return null;

        dynamic? installer = Activator.CreateInstance(t);
        if (installer is null) return null;

        object? db = null, view = null, record = null;
        try
        {
            db = installer.OpenDatabase(filePath, 0); // msiOpenDatabaseModeReadOnly
            dynamic d = db!;
            view = d.OpenView("SELECT `Value` FROM `Property` WHERE `Property`='ProductVersion'");
            dynamic vw = view!;
            vw.Execute(null);
            record = vw.Fetch();
            if (record is null) return null;
            dynamic rec = record;
            string? version = rec.StringData(1);
            return string.IsNullOrWhiteSpace(version) ? null : version!.Trim();
        }
        finally
        {
            ReleaseCom(record);
            ReleaseCom(view);
            ReleaseCom(db);
            ReleaseCom(installer);
        }
    }

    private static void ReleaseCom(object? com)
    {
        if (com is not null && Marshal.IsComObject(com))
            Marshal.FinalReleaseComObject(com);
    }

    /// <summary>
    /// Static validation ("dry run") of a saved payload: does not execute anything.
    /// Returns a list of error messages; empty means it looks runnable.
    /// </summary>
    public static List<string> ValidatePayload(
        string storedPath, string commandFileName, InstallerType type,
        bool isArchive, string successExitCodes)
    {
        var errors = new List<string>();

        foreach (string part in (successExitCodes ?? string.Empty)
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, out _))
                errors.Add($"Ungültiger Erfolgs-Exitcode: '{part}'.");
        }

        if (string.IsNullOrWhiteSpace(commandFileName))
        {
            errors.Add("Keine auszuführende Datei angegeben.");
            return errors;
        }

        if (isArchive)
        {
            try
            {
                using ZipArchive zip = ZipFile.OpenRead(storedPath);
                string wanted = commandFileName.Replace('\\', '/');
                bool found = zip.Entries.Any(e =>
                    e.FullName.Replace('\\', '/').Equals(wanted, StringComparison.OrdinalIgnoreCase)
                    || e.Name.Equals(commandFileName, StringComparison.OrdinalIgnoreCase));
                if (!found)
                {
                    string sample = string.Join(", ", zip.Entries.Take(8).Select(e => e.FullName));
                    errors.Add($"Auszuführende Datei '{commandFileName}' nicht im ZIP gefunden. Enthalten u. a.: {sample}");
                }
            }
            catch (Exception ex)
            {
                errors.Add("ZIP-Archiv konnte nicht gelesen werden: " + ex.Message);
            }
        }
        else
        {
            InstallerType? expected = DetectType(commandFileName);
            if (expected is not null && expected != type)
                errors.Add($"Die Dateiendung passt nicht zum gewählten Installer-Typ ({type}); erwartet wäre eher {expected}.");
        }

        return errors;
    }
}
