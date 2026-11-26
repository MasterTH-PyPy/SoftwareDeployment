using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Services;

/// <summary>
/// Used when the server is not running on Windows / is not domain-joined (e.g.
/// a developer box or a Linux CI build). AD features are reported as unavailable
/// and no SIDs resolve, so no deployments are handed out. The admin UI shows a
/// clear banner in this state.
/// </summary>
public sealed class NullDirectoryService : IDirectoryService
{
    public bool IsAvailable => false;

    public DomainInfo GetDomainInfo()
        => new(false, "(no directory)", null,
               "Server is not running on a domain-joined Windows host.");

    public IReadOnlyList<DirectoryPrincipal> Search(AssignmentTargetType kind, string? filter, int max = 100)
        => [];

    public IReadOnlyCollection<string> GetEffectiveSids(string machineName, string? userName)
        => [];
}
