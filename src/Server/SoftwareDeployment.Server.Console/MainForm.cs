using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Console;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        txtServerDir.Text = AppContext.BaseDirectory;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        cmbAccountType.SelectedIndex = 0;
        UpdateAccountFields();
        RefreshServiceStatus();
        Log("Konsole bereit.");
    }

    // ============================ STATUS ============================
    private void BtnCheck_Click(object? sender, EventArgs e)
    {
        lvChecks.Items.Clear();
        foreach (CheckResult r in DependencyChecker.Run(ServerDir))
        {
            var item = new ListViewItem(r.Name);
            item.SubItems.Add(r.Ok ? "OK" : "FEHLT");
            item.SubItems.Add(r.Detail);
            item.ForeColor = r.Ok ? Color.FromArgb(22, 128, 60) : Color.FromArgb(185, 28, 28);
            lvChecks.Items.Add(item);
        }
        Log("Abhängigkeiten geprüft.");
    }

    // ============================ SERVICE ============================
    private void CmbAccountType_SelectedIndexChanged(object? sender, EventArgs e) => UpdateAccountFields();

    private void UpdateAccountFields()
    {
        switch (cmbAccountType.SelectedIndex)
        {
            case 0: // domain account
                txtAccount.Enabled = true;
                txtPassword.Enabled = true;
                if (string.IsNullOrWhiteSpace(txtAccount.Text)
                    || txtAccount.Text is "LocalSystem" or "NT AUTHORITY\\NetworkService")
                    txtAccount.Text = Environment.UserDomainName + "\\svc_deploy";
                break;
            case 1: // gMSA
                txtAccount.Enabled = true;
                txtPassword.Enabled = false;
                txtPassword.Text = "";
                if (!txtAccount.Text.EndsWith('$'))
                    txtAccount.Text = Environment.UserDomainName + "\\svc_deploy$";
                break;
            case 2: // LocalSystem
                txtAccount.Enabled = false;
                txtPassword.Enabled = false;
                txtAccount.Text = "LocalSystem";
                txtPassword.Text = "";
                break;
            case 3: // NetworkService
                txtAccount.Enabled = false;
                txtPassword.Enabled = false;
                txtAccount.Text = "NT AUTHORITY\\NetworkService";
                txtPassword.Text = "";
                break;
        }
    }

    private (string account, string? password) ResolveAccount() => cmbAccountType.SelectedIndex switch
    {
        2 => ("LocalSystem", null),
        3 => ("NT AUTHORITY\\NetworkService", null),
        1 => (txtAccount.Text.Trim(), null),             // gMSA: empty password
        _ => (txtAccount.Text.Trim(), txtPassword.Text), // domain account
    };

    private void BtnInstall_Click(object? sender, EventArgs e)
    {
        string exe = Path.Combine(ServerDir, DependencyChecker.ServerExeName);
        if (!File.Exists(exe))
        {
            Log($"FEHLER: {DependencyChecker.ServerExeName} nicht in {ServerDir} gefunden.");
            return;
        }
        if (ServiceManager.IsInstalled(DeploymentConstants.ServerServiceName))
        {
            Log("Dienst ist bereits installiert. Nutzen Sie „Konto setzen“ zum Ändern.");
            return;
        }

        (string account, string? password) = ResolveAccount();
        (bool ok, string log) = ServiceManager.Install(
            DeploymentConstants.ServerServiceName,
            DeploymentConstants.ServerServiceDisplayName,
            exe, account, password,
            "SonOG Software Deployment Server (HTTPS-API + Web-Interface).");

        Log((ok ? "OK: " : "FEHLER: ") + log);
        RefreshServiceStatus();
    }

    private void BtnSetAccount_Click(object? sender, EventArgs e)
    {
        if (!ServiceManager.IsInstalled(DeploymentConstants.ServerServiceName))
        {
            Log("Dienst ist nicht installiert.");
            return;
        }
        (string account, string? password) = ResolveAccount();
        (bool ok, string log) = ServiceManager.SetAccount(DeploymentConstants.ServerServiceName, account, password);
        Log((ok ? "OK: Konto gesetzt. " : "FEHLER: ") + log);
    }

    private void BtnUninstall_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("Dienst wirklich entfernen?", "Bestätigen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        (bool ok, string log) = ServiceManager.Uninstall(DeploymentConstants.ServerServiceName);
        Log((ok ? "OK: " : "FEHLER: ") + log);
        RefreshServiceStatus();
    }

    private void BtnStart_Click(object? sender, EventArgs e) => ServiceAction(ServiceManager.Start);
    private void BtnStop_Click(object? sender, EventArgs e) => ServiceAction(ServiceManager.Stop);
    private void BtnRefresh_Click(object? sender, EventArgs e) => RefreshServiceStatus();

    private void ServiceAction(Func<string, (bool ok, string log)> action)
    {
        (bool ok, string log) = action(DeploymentConstants.ServerServiceName);
        Log((ok ? "OK: " : "FEHLER: ") + log);
        RefreshServiceStatus();
    }

    private void RefreshServiceStatus()
        => lblServiceStatus.Text = "Status: " + ServiceManager.GetStatusText(DeploymentConstants.ServerServiceName);

    // ========================= ADMIN PASSWORD =========================
    private void BtnSetAdminPw_Click(object? sender, EventArgs e)
    {
        if (txtAdminPw.Text.Length < 10)
        {
            Log("FEHLER: Kennwort muss mindestens 10 Zeichen haben.");
            return;
        }
        if (txtAdminPw.Text != txtAdminPw2.Text)
        {
            Log("FEHLER: Kennwörter stimmen nicht überein.");
            return;
        }
        try
        {
            string path = SecretsWriter.SetAdminPassword(ServerDir, txtAdminPw.Text);
            txtAdminPw.Text = txtAdminPw2.Text = "";
            Log($"OK: Admin-Kennwort hinterlegt ({path}). Dienst neu starten, damit es greift.");
        }
        catch (Exception ex)
        {
            Log("FEHLER: " + ex.Message);
        }
    }

    // ============================ DIRECTORY ============================
    private void BtnProbe_Click(object? sender, EventArgs e) => txtDomain.Text = DomainProbe.Probe();

    // ============================ helpers ============================
    private string ServerDir => txtServerDir.Text.Trim();

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog { SelectedPath = ServerDir };
        if (dlg.ShowDialog() == DialogResult.OK)
            txtServerDir.Text = dlg.SelectedPath;
    }

    private void Log(string message)
        => txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
}
