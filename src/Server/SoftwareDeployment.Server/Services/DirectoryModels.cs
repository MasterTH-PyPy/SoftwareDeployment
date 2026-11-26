using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Services;

/// <summary>An Active Directory principal that can be the target of an assignment.</summary>
public sealed record DirectoryPrincipal(
    string Sid,
    string SamAccountName,
    string DisplayName,
    string DistinguishedName,
    AssignmentTargetType Kind);

/// <summary>Result of probing the directory the server is joined to.</summary>
public sealed record DomainInfo(
    bool Connected,
    string Name,
    string? DnsName,
    string? Error);
