using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Services;

/// <summary>
/// Read-only access to Active Directory. Used by the admin web interface to pick
/// assignment targets, and by the deployment resolver to work out which
/// computer/group/user SIDs apply to a polling client.
/// </summary>
public interface IDirectoryService
{
    /// <summary>True when the server is domain-joined and AD APIs are usable (Windows only).</summary>
    bool IsAvailable { get; }

    /// <summary>Probe the current domain; never throws.</summary>
    DomainInfo GetDomainInfo();

    /// <summary>Search principals of a kind by (optional) name fragment.</summary>
    IReadOnlyList<DirectoryPrincipal> Search(AssignmentTargetType kind, string? filter, int max = 100);

    /// <summary>
    /// All SIDs that apply to a client: the computer account SID + its recursive
    /// group SIDs, plus the logged-on user SID + its recursive group SIDs. These
    /// are matched against assignment SIDs to decide what the client should get.
    /// </summary>
    IReadOnlyCollection<string> GetEffectiveSids(string machineName, string? userName);
}
