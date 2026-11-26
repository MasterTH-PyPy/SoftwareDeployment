namespace SoftwareDeployment.Shared.Contracts;

/// <summary>Sent once by a fresh client when it first contacts the server.</summary>
public sealed class ClientRegistrationRequest
{
    public string MachineName { get; set; } = string.Empty;

    /// <summary>NetBIOS or DNS domain the machine belongs to (e.g. "PFUNGSTADT").</summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>OS description, e.g. "Microsoft Windows 11 Enterprise 24H2".</summary>
    public string OsVersion { get; set; } = string.Empty;

    /// <summary>Version of the client agent, for the admin overview.</summary>
    public string AgentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Enrollment key entered during client installation. Must match the key
    /// configured on the server, otherwise registration is refused. This gates
    /// who may enroll at all; per-client approval still happens afterwards.
    /// </summary>
    public string EnrollmentKey { get; set; } = string.Empty;
}

/// <summary>Server response to a registration. The client persists id + api key.</summary>
public sealed class ClientRegistrationResponse
{
    public Guid ClientId { get; set; }

    /// <summary>
    /// Secret the client sends on every later call. Only returned once, at
    /// registration time; the server stores just a salted hash.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    public ClientApprovalStatus Status { get; set; }
}
