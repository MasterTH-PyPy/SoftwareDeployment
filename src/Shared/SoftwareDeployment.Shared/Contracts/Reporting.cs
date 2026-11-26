namespace SoftwareDeployment.Shared.Contracts;

/// <summary>Client -> server after it has processed one or more tasks.</summary>
public sealed class InstallReportRequest
{
    public List<InstallResultDto> Results { get; set; } = new();
}

public sealed class InstallResultDto
{
    public int PackageId { get; set; }
    public string Version { get; set; } = string.Empty;

    public InstallResultStatus Status { get; set; }

    /// <summary>Process exit code (null if the process never started).</summary>
    public int? ExitCode { get; set; }

    /// <summary>Free-text detail: last log lines, exception message, etc. (truncated server side).</summary>
    public string? Message { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset FinishedAtUtc { get; set; }
}
