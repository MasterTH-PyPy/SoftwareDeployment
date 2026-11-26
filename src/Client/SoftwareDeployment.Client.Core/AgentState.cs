using System.Text.Json;
using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Client.Core;

/// <summary>Snapshot of what the agent is doing, written by the service and read
/// by the GUI.</summary>
public sealed class AgentState
{
    public ClientApprovalStatus Status { get; set; } = ClientApprovalStatus.Pending;
    public DateTime LastPollUtc { get; set; }
    public int PollIntervalSeconds { get; set; } = 300;
    public string? LastError { get; set; }
    public List<AgentPackageState> Packages { get; set; } = new();
}

public sealed class AgentPackageState
{
    public int PackageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public InstallResultStatus Status { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public string? Message { get; set; }
}

public static class AgentStateStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private static readonly object Gate = new();

    public static AgentState Load()
    {
        try
        {
            if (File.Exists(AgentPaths.StateFile))
                return JsonSerializer.Deserialize<AgentState>(File.ReadAllText(AgentPaths.StateFile))
                       ?? new AgentState();
        }
        catch { /* fall through */ }
        return new AgentState();
    }

    public static void Save(AgentState state)
    {
        AgentPaths.EnsureCreated();
        lock (Gate)
        {
            // Write-then-replace so a reader never sees a half-written file.
            string tmp = AgentPaths.StateFile + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(state, Json));
            File.Move(tmp, AgentPaths.StateFile, overwrite: true);
        }
    }
}
