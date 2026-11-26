namespace SoftwareDeployment.Shared.Contracts;

/// <summary>
/// Body of the recurring poll. The client reports who is logged on and what it
/// already has installed; the server diffs this against the assigned packages.
/// </summary>
public sealed class PollRequest
{
    /// <summary>
    /// SAM account name of the interactive user currently logged on, e.g.
    /// "r.kasper", or null/empty if nobody is logged on (service context).
    /// Used to resolve user- and group-targeted assignments.
    /// </summary>
    public string? CurrentUser { get; set; }

    /// <summary>Agent version, refreshed on every poll so upgrades are visible.</summary>
    public string AgentVersion { get; set; } = string.Empty;

    /// <summary>What the client currently believes is installed (package id -> version).</summary>
    public List<InstalledPackageDto> InstalledPackages { get; set; } = new();
}

public sealed class InstalledPackageDto
{
    public int PackageId { get; set; }
    public string Version { get; set; } = string.Empty;
}

/// <summary>Server response to a poll.</summary>
public sealed class PollResponse
{
    /// <summary>Current approval state. While Pending/Rejected, Tasks is empty.</summary>
    public ClientApprovalStatus Status { get; set; }

    /// <summary>How long the client should wait before polling again, in seconds.</summary>
    public int PollIntervalSeconds { get; set; } = 300;

    /// <summary>Packages that need to be (re)installed on this client right now.</summary>
    public List<DeploymentTaskDto> Tasks { get; set; } = new();
}

/// <summary>A single unit of work handed to the client.</summary>
public sealed class DeploymentTaskDto
{
    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public InstallerType InstallerType { get; set; }

    /// <summary>
    /// File inside the payload to execute (the setup.exe / package.msi / install.bat).
    /// Relative to the extracted payload root.
    /// </summary>
    public string CommandFileName { get; set; } = string.Empty;

    /// <summary>Arguments appended to the install command (e.g. "/qn /norestart").</summary>
    public string Arguments { get; set; } = string.Empty;

    /// <summary>Original payload file name, so the client can name its download.</summary>
    public string PayloadFileName { get; set; } = string.Empty;

    /// <summary>SHA-256 (hex) of the payload, so the client can verify the download.</summary>
    public string PayloadSha256 { get; set; } = string.Empty;

    public long PayloadSize { get; set; }

    /// <summary>
    /// Exit codes that count as success, comma separated (default "0,3010").
    /// 3010 = success, reboot required.
    /// </summary>
    public string SuccessExitCodes { get; set; } = "0,3010";

    /// <summary>
    /// If true the payload is a .zip that must be extracted before running
    /// CommandFileName; if false CommandFileName is the payload itself.
    /// </summary>
    public bool PayloadIsArchive { get; set; }
}
