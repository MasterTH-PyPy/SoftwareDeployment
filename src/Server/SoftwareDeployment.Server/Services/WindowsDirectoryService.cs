using System.DirectoryServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Services;

/// <summary>
/// AD access via LDAP against the domain the server is joined to. Runs under the
/// server's service account, which only needs read access to the directory.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsDirectoryService(ILogger<WindowsDirectoryService> logger) : IDirectoryService
{
    // LDAP matching rule "in chain" -> transitive (recursive) group membership.
    private const string InChain = "1.2.840.113556.1.4.1945";

    public bool IsAvailable => OperatingSystem.IsWindows();

    public DomainInfo GetDomainInfo()
    {
        try
        {
            using var rootDse = new DirectoryEntry("LDAP://RootDSE");
            var defaultNc = rootDse.Properties["defaultNamingContext"].Value?.ToString();
            var dnsHostName = rootDse.Properties["dnsHostName"].Value?.ToString();
            return new DomainInfo(
                Connected: !string.IsNullOrEmpty(defaultNc),
                Name: defaultNc ?? "(unknown)",
                DnsName: dnsHostName,
                Error: null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not read domain info");
            return new DomainInfo(false, "(not connected)", null, ex.Message);
        }
    }

    public IReadOnlyList<DirectoryPrincipal> Search(AssignmentTargetType kind, string? filter, int max = 100)
    {
        var results = new List<DirectoryPrincipal>();
        try
        {
            using var root = new DirectoryEntry("LDAP://" + DefaultNamingContext());
            using var searcher = new DirectorySearcher(root)
            {
                PageSize = Math.Min(max, 1000),
                SizeLimit = max,
                Filter = BuildSearchFilter(kind, filter),
            };
            searcher.PropertiesToLoad.AddRange(
                ["objectSid", "sAMAccountName", "displayName", "cn", "distinguishedName"]);

            using SearchResultCollection found = searcher.FindAll();
            foreach (SearchResult r in found)
            {
                string? sid = ReadSid(r, "objectSid");
                if (sid is null) continue;

                string sam = ReadString(r, "sAMAccountName") ?? string.Empty;
                string display = ReadString(r, "displayName")
                                 ?? ReadString(r, "cn")
                                 ?? sam;
                string dn = ReadString(r, "distinguishedName") ?? string.Empty;

                results.Add(new DirectoryPrincipal(sid, sam, display, dn, kind));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AD search failed (kind={Kind}, filter={Filter})", kind, filter);
        }

        return results
            .OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyCollection<string> GetEffectiveSids(string machineName, string? userName)
    {
        var sids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            string nc = DefaultNamingContext();
            using var root = new DirectoryEntry("LDAP://" + nc);

            // Computer account: sAMAccountName is "NAME$".
            AddPrincipalAndGroups(root, sids,
                $"(&(objectCategory=computer)(sAMAccountName={EscapeFilter(TrimMachine(machineName))}$))");

            // Logged-on user, if any.
            string? sam = NormalizeUser(userName);
            if (!string.IsNullOrEmpty(sam))
            {
                AddPrincipalAndGroups(root, sids,
                    $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={EscapeFilter(sam)}))");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resolving effective SIDs failed for {Machine}/{User}", machineName, userName);
        }
        return sids;
    }

    // ---- helpers ---------------------------------------------------------

    private void AddPrincipalAndGroups(DirectoryEntry root, HashSet<string> sids, string filter)
    {
        using var searcher = new DirectorySearcher(root)
        {
            Filter = filter,
            SizeLimit = 1,
        };
        searcher.PropertiesToLoad.AddRange(["objectSid", "distinguishedName"]);

        SearchResult? hit = searcher.FindOne();
        if (hit is null) return;

        string? ownSid = ReadSid(hit, "objectSid");
        if (ownSid is not null) sids.Add(ownSid);

        string? dn = ReadString(hit, "distinguishedName");
        if (string.IsNullOrEmpty(dn)) return;

        // Transitive group memberships of this DN.
        using var groupSearcher = new DirectorySearcher(root)
        {
            Filter = $"(&(objectCategory=group)(member:{InChain}:={EscapeFilter(dn)}))",
            PageSize = 1000,
        };
        groupSearcher.PropertiesToLoad.Add("objectSid");

        using SearchResultCollection groups = groupSearcher.FindAll();
        foreach (SearchResult g in groups)
        {
            string? gs = ReadSid(g, "objectSid");
            if (gs is not null) sids.Add(gs);
        }
    }

    private string DefaultNamingContext()
    {
        using var rootDse = new DirectoryEntry("LDAP://RootDSE");
        return rootDse.Properties["defaultNamingContext"].Value?.ToString()
               ?? throw new InvalidOperationException("No defaultNamingContext; server is not domain-joined.");
    }

    private static string BuildSearchFilter(AssignmentTargetType kind, string? filter)
    {
        string f = string.IsNullOrWhiteSpace(filter) ? "*" : $"*{EscapeFilter(filter.Trim())}*";
        return kind switch
        {
            AssignmentTargetType.Computer =>
                $"(&(objectCategory=computer)(|(cn={f})(sAMAccountName={f})))",
            AssignmentTargetType.Group =>
                $"(&(objectCategory=group)(|(cn={f})(sAMAccountName={f})))",
            AssignmentTargetType.User =>
                $"(&(objectCategory=person)(objectClass=user)(|(cn={f})(sAMAccountName={f})(displayName={f})))",
            _ => "(objectClass=*)",
        };
    }

    private static string? ReadString(SearchResult r, string prop)
        => r.Properties[prop].Count > 0 ? r.Properties[prop][0]?.ToString() : null;

    private static string? ReadSid(SearchResult r, string prop)
    {
        if (r.Properties[prop].Count == 0) return null;
        if (r.Properties[prop][0] is not byte[] raw) return null;
        return new SecurityIdentifier(raw, 0).ToString();
    }

    /// <summary>Strip domain/UPN decoration, leaving the bare SAM account name.</summary>
    private static string? NormalizeUser(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return null;
        string u = userName.Trim();
        int slash = u.IndexOf('\\');
        if (slash >= 0) u = u[(slash + 1)..];
        int at = u.IndexOf('@');
        if (at >= 0) u = u[..at];
        return u;
    }

    private static string TrimMachine(string machineName)
    {
        string m = machineName.Trim();
        return m.EndsWith('$') ? m[..^1] : m;
    }

    /// <summary>Escape a value for use inside an LDAP filter (RFC 4515).</summary>
    private static string EscapeFilter(string value)
    {
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (char c in value)
        {
            switch (c)
            {
                case '\\': sb.Append("\\5c"); break;
                case '*': sb.Append("\\2a"); break;
                case '(': sb.Append("\\28"); break;
                case ')': sb.Append("\\29"); break;
                case '\0': sb.Append("\\00"); break;
                case '/': sb.Append("\\2f"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }
}
