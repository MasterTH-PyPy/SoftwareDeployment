using System.Text;
using SoftwareDeployment.Client.Core;
using SoftwareDeployment.Shared;
using SoftwareDeployment.Shared.Contracts;

namespace SoftwareDeployment.Client.Gui;

public partial class MainForm : Form
{
    private readonly System.Windows.Forms.Timer _timer;

    public MainForm()
    {
        InitializeComponent();

        AgentConfig cfg = AgentConfigStore.Load();
        txtServer.Text = cfg.ServerBaseUrl;

        _timer = new System.Windows.Forms.Timer { Interval = 3000 };
        _timer.Tick += (_, _) => { RefreshStatus(); RefreshPackages(); };

        Load += (_, _) =>
        {
            RefreshStatus();
            RefreshPackages();
            RefreshLog();
            _timer.Start();
        };
        FormClosed += (_, _) => { _timer.Stop(); _timer.Dispose(); };
    }

    // ---- status ----------------------------------------------------------
    private void BtnStatusRefresh_Click(object? sender, EventArgs e)
    {
        RefreshStatus();
        RefreshPackages();
    }

    private void RefreshStatus()
    {
        AgentConfig cfg = AgentConfigStore.Load();
        AgentState st = AgentStateStore.Load();

        var sb = new StringBuilder();
        sb.AppendLine($"Registrierung:     {(cfg.IsRegistered ? "ja" : "nein")}");
        sb.AppendLine($"Server:            {(string.IsNullOrEmpty(cfg.ServerBaseUrl) ? "—" : cfg.ServerBaseUrl)}");
        sb.AppendLine($"Client-ID:         {(cfg.ClientId?.ToString() ?? "—")}");
        sb.AppendLine($"Rechner:           {ClientInfo.MachineName} ({ClientInfo.Domain})");
        sb.AppendLine($"Agent-Version:     {ClientInfo.AgentVersion}");
        sb.AppendLine();
        sb.AppendLine($"Freigabe-Status:   {StatusText(st.Status)}");
        sb.AppendLine($"Letzter Poll:      {Local(st.LastPollUtc)}");
        sb.AppendLine($"Poll-Intervall:    {st.PollIntervalSeconds} s");
        if (!string.IsNullOrEmpty(st.LastError))
            sb.AppendLine($"Letzter Fehler:    {st.LastError}");

        txtStatus.Text = sb.ToString();
    }

    // ---- packages --------------------------------------------------------
    private void RefreshPackages()
    {
        AgentState st = AgentStateStore.Load();
        lvPackages.BeginUpdate();
        try
        {
            lvPackages.Items.Clear();
            foreach (AgentPackageState p in st.Packages.OrderBy(p => p.Name))
            {
                var item = new ListViewItem(p.Name);
                item.SubItems.Add(p.Version);
                item.SubItems.Add(StatusText(p.Status));
                item.SubItems.Add(Local(p.UpdatedUtc));
                item.SubItems.Add(p.Message ?? "");
                item.ForeColor = p.Status switch
                {
                    InstallResultStatus.Success => Color.FromArgb(22, 128, 60),
                    InstallResultStatus.Failed => Color.FromArgb(185, 28, 28),
                    _ => Color.Black,
                };
                lvPackages.Items.Add(item);
            }
        }
        finally
        {
            lvPackages.EndUpdate();
        }
    }

    // ---- log -------------------------------------------------------------
    private void BtnLogRefresh_Click(object? sender, EventArgs e) => RefreshLog();

    private void RefreshLog()
    {
        txtLog.Lines = AgentLog.Tail(500).ToArray();
        txtLog.SelectionStart = txtLog.TextLength;
        txtLog.ScrollToCaret();
    }

    // ---- registration ----------------------------------------------------
    private async void BtnRegister_Click(object? sender, EventArgs e)
    {
        string url = txtServer.Text.Trim();
        string key = txtKey.Text.Trim();

        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            SetRegResult("Die Server-URL muss mit https:// beginnen (keine unverschlüsselte Verbindung).", true);
            return;
        }
        if (string.IsNullOrEmpty(key))
        {
            SetRegResult("Bitte den Installations-Key eingeben.", true);
            return;
        }

        btnRegister.Enabled = false;
        SetRegResult("Registrierung läuft…", false);
        try
        {
            var request = new ClientRegistrationRequest
            {
                MachineName = ClientInfo.MachineName,
                Domain = ClientInfo.Domain,
                OsVersion = ClientInfo.OsVersion,
                AgentVersion = ClientInfo.AgentVersion,
                EnrollmentKey = key,
            };

            using var api = new DeploymentApiClient(url);
            ClientRegistrationResponse resp = await api.RegisterAsync(request);

            AgentConfigStore.SaveRegistration(url, resp.ClientId, resp.ApiKey);
            txtKey.Text = "";

            SetRegResult(resp.Status == ClientApprovalStatus.Approved
                ? "Registriert und bereits freigegeben. Der Dienst beginnt beim nächsten Poll."
                : "Registriert. Warten auf Freigabe durch den Administrator am Server.", false);

            RefreshStatus();
        }
        catch (Exception ex)
        {
            SetRegResult("Registrierung fehlgeschlagen: " + ex.Message, true);
        }
        finally
        {
            btnRegister.Enabled = true;
        }
    }

    private void SetRegResult(string message, bool error)
    {
        lblRegResult.ForeColor = error ? Color.FromArgb(185, 28, 28) : Color.FromArgb(22, 128, 60);
        lblRegResult.Text = message;
        lblConfigInfo.Text = BuildConfigInfo();
    }

    private static string BuildConfigInfo()
    {
        AgentConfig cfg = AgentConfigStore.Load();
        return cfg.IsRegistered
            ? $"Aktuell registriert bei {cfg.ServerBaseUrl}\r\nClient-ID: {cfg.ClientId}"
            : "Noch nicht registriert.";
    }

    // ---- helpers ---------------------------------------------------------
    private static string StatusText(ClientApprovalStatus s) => s switch
    {
        ClientApprovalStatus.Approved => "Freigegeben",
        ClientApprovalStatus.Rejected => "Abgelehnt",
        _ => "Offen / warten auf Freigabe",
    };

    private static string StatusText(InstallResultStatus s) => s switch
    {
        InstallResultStatus.Success => "Erfolg",
        InstallResultStatus.Failed => "Fehler",
        InstallResultStatus.Skipped => "Übersprungen",
        InstallResultStatus.Running => "Läuft",
        _ => "Ausstehend",
    };

    private static string Local(DateTime utc)
        => utc == default
            ? "—"
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd.MM.yy HH:mm:ss");
}
