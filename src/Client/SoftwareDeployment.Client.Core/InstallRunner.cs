using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using SoftwareDeployment.Shared;
using SoftwareDeployment.Shared.Contracts;

namespace SoftwareDeployment.Client.Core;

/// <summary>
/// Executes a single deployment task on the client: download payload to a work
/// folder, verify its SHA-256, extract if it is an archive, run it according to
/// its installer type, then delete the work folder so the local disk does not
/// fill up (the core reason for moving away from the old GPO copy-and-run script).
/// </summary>
public sealed class InstallRunner(DeploymentApiClient api)
{
    public async Task<InstallResultDto> RunAsync(DeploymentTaskDto task, CancellationToken ct)
    {
        var result = new InstallResultDto
        {
            PackageId = task.PackageId,
            Version = task.Version,
            Status = InstallResultStatus.Running,
            StartedAtUtc = DateTimeOffset.UtcNow,
        };

        string workDir = Path.Combine(AgentPaths.WorkDir, $"{task.PackageId}_{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(workDir);

            string payloadPath = Path.Combine(workDir, SafeName(task.PayloadFileName, $"package-{task.PackageId}.bin"));
            await api.DownloadAsync(task.PackageId, task.Version, payloadPath, ct);

            if (!string.IsNullOrEmpty(task.PayloadSha256))
            {
                string actual = await Sha256HexAsync(payloadPath, ct);
                if (!actual.Equals(task.PayloadSha256, StringComparison.OrdinalIgnoreCase))
                    return Fail(result, "SHA-256 stimmt nicht überein – Download beschädigt.");
            }

            string commandPath;
            if (task.PayloadIsArchive)
            {
                string extractDir = Path.Combine(workDir, "extracted");
                ZipFile.ExtractToDirectory(payloadPath, extractDir, overwriteFiles: true);
                commandPath = Path.Combine(extractDir, task.CommandFileName);
                if (!File.Exists(commandPath))
                    return Fail(result, $"Auszuführende Datei '{task.CommandFileName}' nicht im Archiv gefunden.");
            }
            else
            {
                commandPath = payloadPath;
            }

            (int exitCode, string output) = await ExecuteAsync(task, commandPath, ct);
            result.ExitCode = exitCode;
            result.Message = Trim(output);
            result.Status = IsSuccess(task.SuccessExitCodes, exitCode)
                ? InstallResultStatus.Success
                : InstallResultStatus.Failed;

            AgentLog.Write(result.Status == InstallResultStatus.Success ? "INFO" : "ERROR",
                $"{task.PackageName} {task.Version}: Exitcode {exitCode} -> {result.Status}");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Fail(result, ex.Message);
            AgentLog.Error($"{task.PackageName} {task.Version}: {ex.Message}");
        }
        finally
        {
            result.FinishedAtUtc = DateTimeOffset.UtcNow;
            TryCleanup(workDir);
        }

        return result;
    }

    private static async Task<(int exitCode, string output)> ExecuteAsync(
        DeploymentTaskDto task, string commandPath, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(commandPath) ?? AgentPaths.WorkDir,
        };

        string args = task.Arguments ?? string.Empty;
        switch (task.InstallerType)
        {
            case InstallerType.Msi:
                psi.FileName = "msiexec.exe";
                psi.Arguments = $"/i \"{commandPath}\" {args}".Trim();
                break;
            case InstallerType.Bat:
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/c \"\"{commandPath}\" {args}\"";
                break;
            case InstallerType.PowerShell:
                psi.FileName = "powershell.exe";
                psi.Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{commandPath}\" {args}".Trim();
                break;
            case InstallerType.Exe:
            default:
                psi.FileName = commandPath;
                psi.Arguments = args;
                break;
        }

        using var process = new Process { StartInfo = psi };
        var output = new StringBuilder();
        void Capture(string? line)
        {
            if (line is null) return;
            lock (output)
            {
                if (output.Length < 16_000) output.AppendLine(line);
            }
        }
        process.OutputDataReceived += (_, e) => Capture(e.Data);
        process.ErrorDataReceived += (_, e) => Capture(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(ct);

        return (process.ExitCode, output.ToString());
    }

    private static bool IsSuccess(string successExitCodes, int exitCode)
    {
        foreach (string part in (successExitCodes ?? "0")
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out int code) && code == exitCode) return true;
        }
        return false;
    }

    private static async Task<string> Sha256HexAsync(string path, CancellationToken ct)
    {
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, useAsync: true);
        byte[] hash = await SHA256.HashDataAsync(fs, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string SafeName(string name, string fallback)
    {
        if (string.IsNullOrWhiteSpace(name)) return fallback;
        string clean = Path.GetFileName(name.Replace('\\', '/'));
        return string.IsNullOrWhiteSpace(clean) ? fallback : clean;
    }

    private static string? Trim(string s)
        => string.IsNullOrEmpty(s) ? null : (s.Length <= 8000 ? s : s[^8000..]);

    private static InstallResultDto Fail(InstallResultDto r, string message)
    {
        r.Status = InstallResultStatus.Failed;
        r.Message = message;
        return r;
    }

    private static void TryCleanup(string dir)
    {
        try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
        catch (Exception ex) { AgentLog.Warn($"Arbeitsverzeichnis konnte nicht gelöscht werden: {ex.Message}"); }
    }
}
