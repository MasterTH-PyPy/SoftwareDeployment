namespace SoftwareDeployment.Client.Core;

/// <summary>
/// Fixed file-system locations for the agent, shared by the service (writer) and
/// the GUI (reader). Everything lives under %ProgramData%\SonOG\DeploymentAgent so
/// both the service account and an elevated GUI can reach it.
/// </summary>
public static class AgentPaths
{
    public static string BaseDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "SonOG", "DeploymentAgent");

    public static string ConfigFile => Path.Combine(BaseDir, "config.json");
    public static string StateFile => Path.Combine(BaseDir, "state.json");
    public static string LogDir => Path.Combine(BaseDir, "logs");
    public static string LogFile => Path.Combine(LogDir, "agent.log");
    public static string WorkDir => Path.Combine(BaseDir, "work");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(BaseDir);
        Directory.CreateDirectory(LogDir);
        Directory.CreateDirectory(WorkDir);
    }
}
