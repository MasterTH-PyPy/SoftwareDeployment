using System.ComponentModel;
using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Console;

public sealed class MainForm : Form
{
    private readonly TextBox _txtServerDir;
    private readonly TextBox _log;

    // Service tab
    private ComboBox _cmbAccountType = null!;
    private TextBox _txtAccount = null!;
    private TextBox _txtPassword = null!;
    private Label _lblServiceStatus = null!;

    // Status tab
    private ListView _lvChecks = null!;

    // Admin password tab
    private TextBox _txtAdminPw = null!;
    private TextBox _txtAdminPw2 = null!;

    // Directory tab
    private TextBox _txtDomain = null!;

    public MainForm()
    {
        Text = "SonOG Software Deployment – Server-Konsole";
        Width = 820;
        Height = 660;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
        MinimumSize = new Size(700, 560);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildStatusTab());
        tabs.TabPages.Add(BuildServiceTab());
        tabs.TabPages.Add(BuildAdminPasswordTab());
        tabs.TabPages.Add(BuildDirectoryTab());

        // ---- log panel (bottom) ----
        var logPanel = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(8) };
        _log = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(246, 248, 250),
            Font = new Font("Consolas", 8.5f),
        };
        logPanel.Controls.Add(_log);
        logPanel.Controls.Add(new Label { Text = "Protokoll", Dock = DockStyle.Top, Height = 18 });

        // ---- top panel (server dir) ----
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(8) };
        var lbl = new Label { Text = "Server-Verzeichnis:", AutoSize = true, Location = new Point(8, 16) };
        _txtServerDir = new TextBox
        {
            Location = new Point(130, 12),
            Width = 560,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = AppContext.BaseDirectory,
        };
        var btnBrowse = new Button { Text = "…", Location = new Point(700, 11), Width = 40, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnBrowse.Click += (_, _) => BrowseServerDir();
        topPanel.Controls.AddRange([lbl, _txtServerDir, btnBrowse]);

        Controls.Add(tabs);
        Controls.Add(logPanel);
        Controls.Add(topPanel);

        Load += (_, _) => { RefreshServiceStatus(); Log("Konsole bereit."); };
    }

    // ============================ STATUS TAB ============================
    private TabPage BuildStatusTab()
    {
        var page = new TabPage("Status & Abhängigkeiten") { Padding = new Padding(10) };

        _lvChecks = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
        };
        _lvChecks.Columns.Add("Prüfung", 230);
        _lvChecks.Columns.Add("Status", 70);
        _lvChecks.Columns.Add("Details", 440);

        var bar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 6, 0, 6) };
        var btnCheck = new Button { Text = "Prüfen", Width = 120, Height = 30 };
        btnCheck.Click += (_, _) => RunChecks();
        bar.Controls.Add(btnCheck);

        page.Controls.Add(_lvChecks);
        page.Controls.Add(bar);
        return page;
    }

    private void RunChecks()
    {
        _lvChecks.Items.Clear();
        foreach (CheckResult r in DependencyChecker.Run(ServerDir))
        {
            var item = new ListViewItem(r.Name);
            item.SubItems.Add(r.Ok ? "OK" : "FEHLT");
            item.SubItems.Add(r.Detail);
            item.ForeColor = r.Ok ? Color.FromArgb(22, 128, 60) : Color.FromArgb(185, 28, 28);
            _lvChecks.Items.Add(item);
        }
        Log("Abhängigkeiten geprüft.");
    }

    // ============================ SERVICE TAB ============================
    private TabPage BuildServiceTab()
    {
        var page = new TabPage("Dienst") { Padding = new Padding(14) };
        int y = 14;

        _lblServiceStatus = new Label { Text = "Status: …", AutoSize = true, Location = new Point(14, y), Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        y += 36;

        page.Controls.Add(new Label { Text = "Kontotyp des Dienstes:", AutoSize = true, Location = new Point(14, y + 3) });
        _cmbAccountType = new ComboBox
        {
            Location = new Point(200, y),
            Width = 330,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };
        _cmbAccountType.Items.AddRange(
        [
            "Domänenkonto (DOMAIN\\benutzer) – empfohlen",
            "gMSA (DOMAIN\\name$)",
            "LocalSystem",
            "NetworkService",
        ]);
        _cmbAccountType.SelectedIndex = 0;
        _cmbAccountType.SelectedIndexChanged += (_, _) => UpdateAccountFields();
        page.Controls.Add(_cmbAccountType);
        y += 36;

        page.Controls.Add(new Label { Text = "Konto:", AutoSize = true, Location = new Point(14, y + 3) });
        _txtAccount = new TextBox { Location = new Point(200, y), Width = 330, Text = Environment.UserDomainName + "\\svc_deploy" };
        page.Controls.Add(_txtAccount);
        y += 36;

        page.Controls.Add(new Label { Text = "Kennwort:", AutoSize = true, Location = new Point(14, y + 3) });
        _txtPassword = new TextBox { Location = new Point(200, y), Width = 330, UseSystemPasswordChar = true };
        page.Controls.Add(_txtPassword);
        y += 44;

        var btnInstall = MakeButton("Dienst installieren", 14, y, 160, InstallService);
        var btnSetAcct = MakeButton("Konto setzen", 184, y, 130, SetServiceAccount);
        page.Controls.AddRange([btnInstall, btnSetAcct]);
        y += 42;

        var btnStart = MakeButton("Starten", 14, y, 100, () => ServiceAction(ServiceManager.Start));
        var btnStop = MakeButton("Stoppen", 124, y, 100, () => ServiceAction(ServiceManager.Stop));
        var btnUninstall = MakeButton("Deinstallieren", 234, y, 130, UninstallService);
        var btnRefresh = MakeButton("Status aktualisieren", 374, y, 160, RefreshServiceStatus);
        page.Controls.AddRange([btnStart, btnStop, btnUninstall, btnRefresh]);

        page.Controls.Add(_lblServiceStatus);

        UpdateAccountFields();
        return page;
    }

    private void UpdateAccountFields()
    {
        switch (_cmbAccountType.SelectedIndex)
        {
            case 0: // domain account
                _txtAccount.Enabled = true;
                _txtPassword.Enabled = true;
                if (string.IsNullOrWhiteSpace(_txtAccount.Text) || _txtAccount.Text is "LocalSystem" or "NT AUTHORITY\\NetworkService")
                    _txtAccount.Text = Environment.UserDomainName + "\\svc_deploy";
                break;
            case 1: // gMSA
                _txtAccount.Enabled = true;
                _txtPassword.Enabled = false;
                _txtPassword.Text = "";
                if (!_txtAccount.Text.EndsWith('$')) _txtAccount.Text = Environment.UserDomainName + "\\svc_deploy$";
                break;
            case 2: // LocalSystem
                _txtAccount.Enabled = false;
                _txtPassword.Enabled = false;
                _txtAccount.Text = "LocalSystem";
                _txtPassword.Text = "";
                break;
            case 3: // NetworkService
                _txtAccount.Enabled = false;
                _txtPassword.Enabled = false;
                _txtAccount.Text = "NT AUTHORITY\\NetworkService";
                _txtPassword.Text = "";
                break;
        }
    }

    private (string account, string? password) ResolveAccount() => _cmbAccountType.SelectedIndex switch
    {
        2 => ("LocalSystem", null),
        3 => ("NT AUTHORITY\\NetworkService", null),
        1 => (_txtAccount.Text.Trim(), null),             // gMSA: empty password
        _ => (_txtAccount.Text.Trim(), _txtPassword.Text), // domain account
    };

    private void InstallService()
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

    private void SetServiceAccount()
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

    private void UninstallService()
    {
        if (MessageBox.Show("Dienst wirklich entfernen?", "Bestätigen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        (bool ok, string log) = ServiceManager.Uninstall(DeploymentConstants.ServerServiceName);
        Log((ok ? "OK: " : "FEHLER: ") + log);
        RefreshServiceStatus();
    }

    private void ServiceAction(Func<string, (bool ok, string log)> action)
    {
        (bool ok, string log) = action(DeploymentConstants.ServerServiceName);
        Log((ok ? "OK: " : "FEHLER: ") + log);
        RefreshServiceStatus();
    }

    private void RefreshServiceStatus()
        => _lblServiceStatus.Text = "Status: " + ServiceManager.GetStatusText(DeploymentConstants.ServerServiceName);

    // ========================= ADMIN PASSWORD TAB =========================
    private TabPage BuildAdminPasswordTab()
    {
        var page = new TabPage("Admin-Kennwort") { Padding = new Padding(14) };
        int y = 14;

        page.Controls.Add(new Label
        {
            Text = "Setzt das Administrator-Kennwort für das Web-Interface. Es wird nur als\n" +
                   "PBKDF2-Hash in server-secrets.json geschrieben und vom Server beim Start übernommen.",
            AutoSize = true,
            Location = new Point(14, y),
        });
        y += 56;

        page.Controls.Add(new Label { Text = "Neues Kennwort:", AutoSize = true, Location = new Point(14, y + 3) });
        _txtAdminPw = new TextBox { Location = new Point(200, y), Width = 300, UseSystemPasswordChar = true };
        page.Controls.Add(_txtAdminPw);
        y += 36;

        page.Controls.Add(new Label { Text = "Bestätigen:", AutoSize = true, Location = new Point(14, y + 3) });
        _txtAdminPw2 = new TextBox { Location = new Point(200, y), Width = 300, UseSystemPasswordChar = true };
        page.Controls.Add(_txtAdminPw2);
        y += 44;

        page.Controls.Add(MakeButton("Kennwort setzen", 14, y, 160, SetAdminPassword));
        return page;
    }

    private void SetAdminPassword()
    {
        if (_txtAdminPw.Text.Length < 10)
        {
            Log("FEHLER: Kennwort muss mindestens 10 Zeichen haben.");
            return;
        }
        if (_txtAdminPw.Text != _txtAdminPw2.Text)
        {
            Log("FEHLER: Kennwörter stimmen nicht überein.");
            return;
        }
        try
        {
            string path = SecretsWriter.SetAdminPassword(ServerDir, _txtAdminPw.Text);
            _txtAdminPw.Text = _txtAdminPw2.Text = "";
            Log($"OK: Admin-Kennwort hinterlegt ({path}). Dienst neu starten, damit es greift.");
        }
        catch (Exception ex)
        {
            Log("FEHLER: " + ex.Message);
        }
    }

    // ============================ DIRECTORY TAB ============================
    private TabPage BuildDirectoryTab()
    {
        var page = new TabPage("Verzeichnis (AD)") { Padding = new Padding(14) };

        var bar = new Panel { Dock = DockStyle.Top, Height = 44 };
        var btn = new Button { Text = "Domäne prüfen", Width = 160, Height = 30 };
        btn.Click += (_, _) => _txtDomain.Text = DomainProbe.Probe();
        bar.Controls.Add(btn);

        _txtDomain = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9f),
        };

        page.Controls.Add(_txtDomain);
        page.Controls.Add(bar);
        return page;
    }

    // ============================ helpers ============================
    private string ServerDir => _txtServerDir.Text.Trim();

    private static Button MakeButton(string text, int x, int y, int width, Action onClick)
    {
        var b = new Button { Text = text, Location = new Point(x, y), Width = width, Height = 32 };
        b.Click += (_, _) => onClick();
        return b;
    }

    private void BrowseServerDir()
    {
        using var dlg = new FolderBrowserDialog { SelectedPath = ServerDir };
        if (dlg.ShowDialog() == DialogResult.OK)
            _txtServerDir.Text = dlg.SelectedPath;
    }

    private void Log(string message)
    {
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
