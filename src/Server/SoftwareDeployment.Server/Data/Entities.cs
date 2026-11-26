using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Data;

/// <summary>A machine running the client agent.</summary>
public sealed class ClientEntity
{
    public Guid Id { get; set; }

    public string MachineName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;

    /// <summary>Salted PBKDF2 hash of the api key handed out at registration.</summary>
    public string ApiKeyHash { get; set; } = string.Empty;

    public ClientApprovalStatus Status { get; set; } = ClientApprovalStatus.Pending;

    public DateTime RegisteredAtUtc { get; set; }
    public DateTime LastSeenUtc { get; set; } // UTC; DateTime for reliable SQLite translation

    /// <summary>SAM account name of the user seen on the most recent poll.</summary>
    public string? LastKnownUser { get; set; }

    public ICollection<ClientPackageStateEntity> PackageStates { get; set; } = new List<ClientPackageStateEntity>();
    public ICollection<InstallLogEntity> Logs { get; set; } = new List<InstallLogEntity>();
}

/// <summary>A deployable piece of software. Holds one or more versions.</summary>
public sealed class PackageEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<PackageVersionEntity> Versions { get; set; } = new List<PackageVersionEntity>();
    public ICollection<AssignmentEntity> Assignments { get; set; } = new List<AssignmentEntity>();
}

/// <summary>A concrete version of a package, with its payload and run instructions.</summary>
public sealed class PackageVersionEntity
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public PackageEntity Package { get; set; } = null!;

    public string Version { get; set; } = string.Empty;

    public InstallerType InstallerType { get; set; }
    public string CommandFileName { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string SuccessExitCodes { get; set; } = "0,3010";
    public bool PayloadIsArchive { get; set; }

    /// <summary>Original uploaded file name (for display / download naming).</summary>
    public string PayloadFileName { get; set; } = string.Empty;

    /// <summary>File name on the server's storage disk (a GUID, to avoid clashes).</summary>
    public string PayloadStoredName { get; set; } = string.Empty;

    public string PayloadSha256 { get; set; } = string.Empty;
    public long PayloadSize { get; set; }

    /// <summary>Exactly one version per package is the one currently rolled out.</summary>
    public bool IsCurrent { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>Links a package to an AD principal (computer / group / user) by SID.</summary>
public sealed class AssignmentEntity
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public PackageEntity Package { get; set; } = null!;

    public AssignmentTargetType TargetType { get; set; }

    /// <summary>The principal's SID — stable even if it is renamed.</summary>
    public string TargetSid { get; set; } = string.Empty;

    /// <summary>Display name captured at assignment time (for the admin UI).</summary>
    public string TargetName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>Current installed state of one package on one client (for the overview).</summary>
public sealed class ClientPackageStateEntity
{
    public long Id { get; set; }
    public Guid ClientId { get; set; }
    public ClientEntity Client { get; set; } = null!;

    public int PackageId { get; set; }
    public string InstalledVersion { get; set; } = string.Empty;
    public InstallResultStatus Status { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>Historical log line for one install attempt.</summary>
public sealed class InstallLogEntity
{
    public long Id { get; set; }
    public Guid ClientId { get; set; }
    public ClientEntity Client { get; set; } = null!;

    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public InstallResultStatus Status { get; set; }
    public int? ExitCode { get; set; }
    public string? Message { get; set; }

    public DateTime StartedAtUtc { get; set; }
    public DateTime FinishedAtUtc { get; set; }
    public DateTime ReportedAtUtc { get; set; }
}

/// <summary>Single-row table holding global server configuration.</summary>
public sealed class SettingsEntity
{
    public int Id { get; set; } = 1;

    public int PollIntervalSeconds { get; set; } = 300;
    public bool AutoApproveClients { get; set; }
    public string OrganizationName { get; set; } = "Software Deployment";

    /// <summary>Admin password for the web interface (PBKDF2 hash).</summary>
    public string? AdminPasswordHash { get; set; }

    /// <summary>
    /// Enrollment key required at client installation, stored DPAPI-protected
    /// (never in clear text). Decrypted only to display it in the admin UI and
    /// to verify incoming registrations.
    /// </summary>
    public string? EnrollmentKeyProtected { get; set; }

    /// <summary>Last bootstrap-secrets revision applied, so we re-seed only on change.</summary>
    public int AppliedSecretsRevision { get; set; }
}
