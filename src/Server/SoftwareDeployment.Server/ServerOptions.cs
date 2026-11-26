namespace SoftwareDeployment.Server;

/// <summary>
/// File-system layout, bound from the "Server" section of appsettings.json.
/// All paths default to a "data" folder next to the executable so the server
/// works out-of-the-box and the installer can relocate it with one setting.
/// </summary>
public sealed class ServerOptions
{
    public const string SectionName = "Server";

    /// <summary>Root for all mutable runtime state (DB + package payloads).</summary>
    public string DataDirectory { get; set; } = "data";

    /// <summary>SQLite database file name inside <see cref="DataDirectory"/>.</summary>
    public string DatabaseFileName { get; set; } = "deployment.db";

    /// <summary>Sub-folder of <see cref="DataDirectory"/> holding uploaded payloads.</summary>
    public string PackagesSubDirectory { get; set; } = "packages";

    /// <summary>Upper bound for an uploaded payload, in megabytes.</summary>
    public long MaxUploadMegabytes { get; set; } = 4096;

    public string ResolveDataDirectory(string contentRoot)
        => Path.IsPathRooted(DataDirectory)
            ? DataDirectory
            : Path.GetFullPath(Path.Combine(contentRoot, DataDirectory));

    public string ResolveDatabasePath(string contentRoot)
        => Path.Combine(ResolveDataDirectory(contentRoot), DatabaseFileName);

    public string ResolvePackagesDirectory(string contentRoot)
        => Path.Combine(ResolveDataDirectory(contentRoot), PackagesSubDirectory);
}
