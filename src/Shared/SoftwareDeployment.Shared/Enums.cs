namespace SoftwareDeployment.Shared;

/// <summary>
/// How an install command is executed on the client. Mirrors the original
/// GPO script, which supported exe / msiexec / bat.
/// </summary>
public enum InstallerType
{
    /// <summary>Run an executable directly with arguments.</summary>
    Exe = 0,

    /// <summary>Install an MSI via <c>msiexec /i &lt;file&gt; &lt;args&gt;</c>.</summary>
    Msi = 1,

    /// <summary>Run a batch file through <c>cmd.exe /c</c>.</summary>
    Bat = 2,

    /// <summary>Run a PowerShell script through <c>powershell.exe</c>.</summary>
    PowerShell = 3,
}

/// <summary>Approval state of a registered client. New clients start as Pending.</summary>
public enum ClientApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
}

/// <summary>Kind of Active Directory principal a package assignment targets.</summary>
public enum AssignmentTargetType
{
    Computer = 0,
    Group = 1,
    User = 2,
}

/// <summary>Outcome of a single install attempt, reported back by the client.</summary>
public enum InstallResultStatus
{
    Pending = 0,
    Running = 1,
    Success = 2,
    Failed = 3,
    /// <summary>Install was skipped because the installed version already matched.</summary>
    Skipped = 4,
}
