namespace SoftwareDeployment.Client.Core;

/// <summary>
/// Minimal rolling file logger. The service writes; the GUI tails the file.
/// Keeps the log from growing without bound (simple size-based roll).
/// </summary>
public static class AgentLog
{
    private const long MaxBytes = 2 * 1024 * 1024;
    private static readonly object Gate = new();

    public static void Write(string level, string message)
    {
        try
        {
            AgentPaths.EnsureCreated();
            lock (Gate)
            {
                RollIfNeeded();
                File.AppendAllText(AgentPaths.LogFile,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}");
            }
        }
        catch { /* logging must never throw */ }
    }

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);

    /// <summary>Return up to <paramref name="maxLines"/> last log lines for the GUI.</summary>
    public static IReadOnlyList<string> Tail(int maxLines = 300)
    {
        try
        {
            if (!File.Exists(AgentPaths.LogFile)) return [];
            lock (Gate)
            {
                string[] all = File.ReadAllLines(AgentPaths.LogFile);
                return all.Length <= maxLines ? all : all[^maxLines..];
            }
        }
        catch { return []; }
    }

    private static void RollIfNeeded()
    {
        var fi = new FileInfo(AgentPaths.LogFile);
        if (fi.Exists && fi.Length > MaxBytes)
        {
            string bak = AgentPaths.LogFile + ".1";
            if (File.Exists(bak)) File.Delete(bak);
            File.Move(AgentPaths.LogFile, bak);
        }
    }
}
