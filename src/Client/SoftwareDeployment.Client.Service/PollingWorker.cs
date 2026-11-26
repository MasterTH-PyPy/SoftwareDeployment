using Microsoft.Extensions.Hosting;
using SoftwareDeployment.Client.Core;
using SoftwareDeployment.Shared;
using SoftwareDeployment.Shared.Contracts;

namespace SoftwareDeployment.Client.Service;

/// <summary>
/// The agent loop: every interval, ask the server for due packages, install them
/// (EXE/BAT/PS in parallel, MSI serialized), report results, and persist state for
/// the GUI.
/// </summary>
public sealed class PollingWorker : BackgroundService
{
    private const int MaxParallel = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AgentPaths.EnsureCreated();
        AgentLog.Info("Agent-Dienst gestartet.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int interval = DeploymentConstants.DefaultPollIntervalSeconds;
            try
            {
                interval = await PollOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                AgentLog.Error("Poll fehlgeschlagen: " + ex.Message);
                SaveError(ex.Message);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(interval, 30, 86_400)), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        AgentLog.Info("Agent-Dienst gestoppt.");
    }

    private static async Task<int> PollOnceAsync(CancellationToken ct)
    {
        AgentConfig cfg = AgentConfigStore.Load();
        if (!cfg.IsRegistered)
        {
            AgentLog.Warn("Nicht registriert – in der Agent-GUI Server-URL und Enrollment-Key eingeben.");
            AgentState notReg = AgentStateStore.Load();
            notReg.Status = ClientApprovalStatus.Pending;
            notReg.LastError = "Nicht registriert.";
            notReg.LastPollUtc = DateTime.UtcNow;
            AgentStateStore.Save(notReg);
            return DeploymentConstants.DefaultPollIntervalSeconds;
        }

        string apiKey = AgentConfigStore.GetApiKey(cfg)!;
        using var api = new DeploymentApiClient(cfg.ServerBaseUrl, cfg.ClientId, apiKey);

        AgentState state = AgentStateStore.Load();

        var pollRequest = new PollRequest
        {
            CurrentUser = ClientInfo.GetConsoleUser(),
            AgentVersion = ClientInfo.AgentVersion,
            InstalledPackages = state.Packages
                .Where(p => p.Status == InstallResultStatus.Success)
                .Select(p => new InstalledPackageDto { PackageId = p.PackageId, Version = p.Version })
                .ToList(),
        };

        PollResponse resp = await api.PollAsync(pollRequest, ct);

        state.Status = resp.Status;
        state.PollIntervalSeconds = resp.PollIntervalSeconds;
        state.LastPollUtc = DateTime.UtcNow;
        state.LastError = null;

        if (resp.Status != ClientApprovalStatus.Approved)
        {
            AgentLog.Info($"Status vom Server: {resp.Status} – keine Aufgaben.");
            AgentStateStore.Save(state);
            return resp.PollIntervalSeconds;
        }

        if (resp.Tasks.Count > 0)
        {
            AgentLog.Info($"{resp.Tasks.Count} Aufgabe(n) erhalten.");
            var runner = new InstallRunner(api);
            IReadOnlyList<InstallResultDto> results = await RunTasksAsync(runner, resp.Tasks, ct);

            var names = resp.Tasks.ToDictionary(t => t.PackageId, t => t.PackageName);
            foreach (InstallResultDto r in results)
                UpsertPackageState(state, r, names.GetValueOrDefault(r.PackageId, $"#{r.PackageId}"));

            try
            {
                await api.ReportAsync(new InstallReportRequest { Results = results.ToList() }, ct);
            }
            catch (Exception ex)
            {
                AgentLog.Warn("Report an Server fehlgeschlagen: " + ex.Message);
            }
        }

        AgentStateStore.Save(state);
        return resp.PollIntervalSeconds;
    }

    /// <summary>EXE/BAT/PS run concurrently (bounded); MSI runs strictly one at a
    /// time because Windows Installer only allows one install machine-wide.</summary>
    private static async Task<IReadOnlyList<InstallResultDto>> RunTasksAsync(
        InstallRunner runner, IReadOnlyList<DeploymentTaskDto> tasks, CancellationToken ct)
    {
        using var concurrency = new SemaphoreSlim(MaxParallel, MaxParallel);
        using var msiGate = new SemaphoreSlim(1, 1);

        async Task<InstallResultDto> RunOne(DeploymentTaskDto t)
        {
            bool isMsi = t.InstallerType == InstallerType.Msi;
            // Acquire the MSI gate first so a waiting MSI does not hold a parallel slot.
            if (isMsi) await msiGate.WaitAsync(ct);
            try
            {
                await concurrency.WaitAsync(ct);
                try
                {
                    return await runner.RunAsync(t, ct);
                }
                finally
                {
                    concurrency.Release();
                }
            }
            finally
            {
                if (isMsi) msiGate.Release();
            }
        }

        return await Task.WhenAll(tasks.Select(RunOne));
    }

    private static void UpsertPackageState(AgentState state, InstallResultDto r, string name)
    {
        AgentPackageState? ps = state.Packages.FirstOrDefault(p => p.PackageId == r.PackageId);
        if (ps is null)
        {
            ps = new AgentPackageState { PackageId = r.PackageId };
            state.Packages.Add(ps);
        }
        ps.Name = name;
        ps.Status = r.Status;
        ps.Message = r.Message;
        ps.UpdatedUtc = DateTime.UtcNow;
        if (r.Status is InstallResultStatus.Success or InstallResultStatus.Skipped)
            ps.Version = r.Version;
    }

    private static void SaveError(string message)
    {
        try
        {
            AgentState s = AgentStateStore.Load();
            s.LastError = message;
            s.LastPollUtc = DateTime.UtcNow;
            AgentStateStore.Save(s);
        }
        catch { /* ignore */ }
    }
}
