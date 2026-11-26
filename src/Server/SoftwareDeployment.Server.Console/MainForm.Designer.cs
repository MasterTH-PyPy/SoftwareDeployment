namespace SoftwareDeployment.Server.Console;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.Label lblServerDir;
    private System.Windows.Forms.TextBox txtServerDir;
    private System.Windows.Forms.Button btnBrowse;

    private System.Windows.Forms.Panel panelLog;
    private System.Windows.Forms.Label lblLog;
    private System.Windows.Forms.TextBox txtLog;

    private System.Windows.Forms.TabControl tabs;
    private System.Windows.Forms.TabPage tabStatus;
    private System.Windows.Forms.TabPage tabService;
    private System.Windows.Forms.TabPage tabAdmin;
    private System.Windows.Forms.TabPage tabDirectory;

    private System.Windows.Forms.Panel panelStatusBar;
    private System.Windows.Forms.Button btnCheck;
    private System.Windows.Forms.ListView lvChecks;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.ColumnHeader colStatus;
    private System.Windows.Forms.ColumnHeader colDetail;

    private System.Windows.Forms.Label lblServiceStatus;
    private System.Windows.Forms.Label lblAccountType;
    private System.Windows.Forms.ComboBox cmbAccountType;
    private System.Windows.Forms.Label lblAccount;
    private System.Windows.Forms.TextBox txtAccount;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Button btnInstall;
    private System.Windows.Forms.Button btnSetAccount;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.Button btnUninstall;
    private System.Windows.Forms.Button btnRefresh;

    private System.Windows.Forms.Label lblAdminInfo;
    private System.Windows.Forms.Label lblNewPw;
    private System.Windows.Forms.TextBox txtAdminPw;
    private System.Windows.Forms.Label lblConfirm;
    private System.Windows.Forms.TextBox txtAdminPw2;
    private System.Windows.Forms.Button btnSetAdminPw;

    private System.Windows.Forms.Panel panelDirBar;
    private System.Windows.Forms.Button btnProbe;
    private System.Windows.Forms.TextBox txtDomain;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.panelTop = new System.Windows.Forms.Panel();
        this.lblServerDir = new System.Windows.Forms.Label();
        this.txtServerDir = new System.Windows.Forms.TextBox();
        this.btnBrowse = new System.Windows.Forms.Button();
        this.panelLog = new System.Windows.Forms.Panel();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.lblLog = new System.Windows.Forms.Label();
        this.tabs = new System.Windows.Forms.TabControl();
        this.tabStatus = new System.Windows.Forms.TabPage();
        this.lvChecks = new System.Windows.Forms.ListView();
        this.colName = new System.Windows.Forms.ColumnHeader();
        this.colStatus = new System.Windows.Forms.ColumnHeader();
        this.colDetail = new System.Windows.Forms.ColumnHeader();
        this.panelStatusBar = new System.Windows.Forms.Panel();
        this.btnCheck = new System.Windows.Forms.Button();
        this.tabService = new System.Windows.Forms.TabPage();
        this.lblServiceStatus = new System.Windows.Forms.Label();
        this.lblAccountType = new System.Windows.Forms.Label();
        this.cmbAccountType = new System.Windows.Forms.ComboBox();
        this.lblAccount = new System.Windows.Forms.Label();
        this.txtAccount = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.btnInstall = new System.Windows.Forms.Button();
        this.btnSetAccount = new System.Windows.Forms.Button();
        this.btnStart = new System.Windows.Forms.Button();
        this.btnStop = new System.Windows.Forms.Button();
        this.btnUninstall = new System.Windows.Forms.Button();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.tabAdmin = new System.Windows.Forms.TabPage();
        this.lblAdminInfo = new System.Windows.Forms.Label();
        this.lblNewPw = new System.Windows.Forms.Label();
        this.txtAdminPw = new System.Windows.Forms.TextBox();
        this.lblConfirm = new System.Windows.Forms.Label();
        this.txtAdminPw2 = new System.Windows.Forms.TextBox();
        this.btnSetAdminPw = new System.Windows.Forms.Button();
        this.tabDirectory = new System.Windows.Forms.TabPage();
        this.txtDomain = new System.Windows.Forms.TextBox();
        this.panelDirBar = new System.Windows.Forms.Panel();
        this.btnProbe = new System.Windows.Forms.Button();
        this.panelTop.SuspendLayout();
        this.panelLog.SuspendLayout();
        this.tabs.SuspendLayout();
        this.tabStatus.SuspendLayout();
        this.panelStatusBar.SuspendLayout();
        this.tabService.SuspendLayout();
        this.tabAdmin.SuspendLayout();
        this.tabDirectory.SuspendLayout();
        this.panelDirBar.SuspendLayout();
        this.SuspendLayout();
        //
        // panelTop
        //
        this.panelTop.Controls.Add(this.btnBrowse);
        this.panelTop.Controls.Add(this.txtServerDir);
        this.panelTop.Controls.Add(this.lblServerDir);
        this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelTop.Name = "panelTop";
        this.panelTop.Padding = new System.Windows.Forms.Padding(8);
        this.panelTop.Size = new System.Drawing.Size(804, 54);
        this.panelTop.TabIndex = 2;
        //
        // lblServerDir
        //
        this.lblServerDir.AutoSize = true;
        this.lblServerDir.Location = new System.Drawing.Point(8, 16);
        this.lblServerDir.Name = "lblServerDir";
        this.lblServerDir.Size = new System.Drawing.Size(110, 15);
        this.lblServerDir.TabIndex = 0;
        this.lblServerDir.Text = "Server-Verzeichnis:";
        //
        // txtServerDir
        //
        this.txtServerDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
        this.txtServerDir.Location = new System.Drawing.Point(130, 12);
        this.txtServerDir.Name = "txtServerDir";
        this.txtServerDir.Size = new System.Drawing.Size(620, 23);
        this.txtServerDir.TabIndex = 1;
        //
        // btnBrowse
        //
        this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnBrowse.Location = new System.Drawing.Point(756, 11);
        this.btnBrowse.Name = "btnBrowse";
        this.btnBrowse.Size = new System.Drawing.Size(40, 25);
        this.btnBrowse.TabIndex = 2;
        this.btnBrowse.Text = "…";
        this.btnBrowse.UseVisualStyleBackColor = true;
        this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
        //
        // panelLog
        //
        this.panelLog.Controls.Add(this.txtLog);
        this.panelLog.Controls.Add(this.lblLog);
        this.panelLog.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelLog.Location = new System.Drawing.Point(0, 481);
        this.panelLog.Name = "panelLog";
        this.panelLog.Padding = new System.Windows.Forms.Padding(8);
        this.panelLog.Size = new System.Drawing.Size(804, 140);
        this.panelLog.TabIndex = 1;
        //
        // txtLog
        //
        this.txtLog.BackColor = System.Drawing.Color.FromArgb(246, 248, 250);
        this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtLog.Font = new System.Drawing.Font("Consolas", 8.5F);
        this.txtLog.Location = new System.Drawing.Point(8, 26);
        this.txtLog.Multiline = true;
        this.txtLog.Name = "txtLog";
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.Size = new System.Drawing.Size(788, 106);
        this.txtLog.TabIndex = 1;
        //
        // lblLog
        //
        this.lblLog.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblLog.Location = new System.Drawing.Point(8, 8);
        this.lblLog.Name = "lblLog";
        this.lblLog.Size = new System.Drawing.Size(788, 18);
        this.lblLog.TabIndex = 0;
        this.lblLog.Text = "Protokoll";
        //
        // tabs
        //
        this.tabs.Controls.Add(this.tabStatus);
        this.tabs.Controls.Add(this.tabService);
        this.tabs.Controls.Add(this.tabAdmin);
        this.tabs.Controls.Add(this.tabDirectory);
        this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabs.Location = new System.Drawing.Point(0, 54);
        this.tabs.Name = "tabs";
        this.tabs.SelectedIndex = 0;
        this.tabs.Size = new System.Drawing.Size(804, 427);
        this.tabs.TabIndex = 0;
        //
        // tabStatus
        //
        this.tabStatus.Controls.Add(this.lvChecks);
        this.tabStatus.Controls.Add(this.panelStatusBar);
        this.tabStatus.Location = new System.Drawing.Point(4, 24);
        this.tabStatus.Name = "tabStatus";
        this.tabStatus.Padding = new System.Windows.Forms.Padding(10);
        this.tabStatus.Size = new System.Drawing.Size(796, 399);
        this.tabStatus.TabIndex = 0;
        this.tabStatus.Text = "Status & Abhängigkeiten";
        this.tabStatus.UseVisualStyleBackColor = true;
        //
        // lvChecks
        //
        this.lvChecks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colStatus,
            this.colDetail});
        this.lvChecks.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lvChecks.FullRowSelect = true;
        this.lvChecks.GridLines = true;
        this.lvChecks.Location = new System.Drawing.Point(10, 54);
        this.lvChecks.Name = "lvChecks";
        this.lvChecks.Size = new System.Drawing.Size(776, 335);
        this.lvChecks.TabIndex = 1;
        this.lvChecks.UseCompatibleStateImageBehavior = false;
        this.lvChecks.View = System.Windows.Forms.View.Details;
        //
        // colName
        //
        this.colName.Text = "Prüfung";
        this.colName.Width = 230;
        //
        // colStatus
        //
        this.colStatus.Text = "Status";
        this.colStatus.Width = 70;
        //
        // colDetail
        //
        this.colDetail.Text = "Details";
        this.colDetail.Width = 440;
        //
        // panelStatusBar
        //
        this.panelStatusBar.Controls.Add(this.btnCheck);
        this.panelStatusBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelStatusBar.Location = new System.Drawing.Point(10, 10);
        this.panelStatusBar.Name = "panelStatusBar";
        this.panelStatusBar.Size = new System.Drawing.Size(776, 44);
        this.panelStatusBar.TabIndex = 0;
        //
        // btnCheck
        //
        this.btnCheck.Location = new System.Drawing.Point(0, 7);
        this.btnCheck.Name = "btnCheck";
        this.btnCheck.Size = new System.Drawing.Size(120, 30);
        this.btnCheck.TabIndex = 0;
        this.btnCheck.Text = "Prüfen";
        this.btnCheck.UseVisualStyleBackColor = true;
        this.btnCheck.Click += new System.EventHandler(this.BtnCheck_Click);
        //
        // tabService
        //
        this.tabService.Controls.Add(this.btnRefresh);
        this.tabService.Controls.Add(this.btnUninstall);
        this.tabService.Controls.Add(this.btnStop);
        this.tabService.Controls.Add(this.btnStart);
        this.tabService.Controls.Add(this.btnSetAccount);
        this.tabService.Controls.Add(this.btnInstall);
        this.tabService.Controls.Add(this.txtPassword);
        this.tabService.Controls.Add(this.lblPassword);
        this.tabService.Controls.Add(this.txtAccount);
        this.tabService.Controls.Add(this.lblAccount);
        this.tabService.Controls.Add(this.cmbAccountType);
        this.tabService.Controls.Add(this.lblAccountType);
        this.tabService.Controls.Add(this.lblServiceStatus);
        this.tabService.Location = new System.Drawing.Point(4, 24);
        this.tabService.Name = "tabService";
        this.tabService.Padding = new System.Windows.Forms.Padding(14);
        this.tabService.Size = new System.Drawing.Size(796, 399);
        this.tabService.TabIndex = 1;
        this.tabService.Text = "Dienst";
        this.tabService.UseVisualStyleBackColor = true;
        //
        // lblServiceStatus
        //
        this.lblServiceStatus.AutoSize = true;
        this.lblServiceStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.lblServiceStatus.Location = new System.Drawing.Point(14, 14);
        this.lblServiceStatus.Name = "lblServiceStatus";
        this.lblServiceStatus.Size = new System.Drawing.Size(60, 19);
        this.lblServiceStatus.TabIndex = 0;
        this.lblServiceStatus.Text = "Status: …";
        //
        // lblAccountType
        //
        this.lblAccountType.AutoSize = true;
        this.lblAccountType.Location = new System.Drawing.Point(14, 56);
        this.lblAccountType.Name = "lblAccountType";
        this.lblAccountType.Size = new System.Drawing.Size(130, 15);
        this.lblAccountType.TabIndex = 1;
        this.lblAccountType.Text = "Kontotyp des Dienstes:";
        //
        // cmbAccountType
        //
        this.cmbAccountType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbAccountType.Items.AddRange(new object[] {
            "Domänenkonto (DOMAIN\\benutzer) – empfohlen",
            "gMSA (DOMAIN\\name$)",
            "LocalSystem",
            "NetworkService"});
        this.cmbAccountType.Location = new System.Drawing.Point(200, 53);
        this.cmbAccountType.Name = "cmbAccountType";
        this.cmbAccountType.Size = new System.Drawing.Size(330, 23);
        this.cmbAccountType.TabIndex = 2;
        this.cmbAccountType.SelectedIndexChanged += new System.EventHandler(this.CmbAccountType_SelectedIndexChanged);
        //
        // lblAccount
        //
        this.lblAccount.AutoSize = true;
        this.lblAccount.Location = new System.Drawing.Point(14, 92);
        this.lblAccount.Name = "lblAccount";
        this.lblAccount.Size = new System.Drawing.Size(47, 15);
        this.lblAccount.TabIndex = 3;
        this.lblAccount.Text = "Konto:";
        //
        // txtAccount
        //
        this.txtAccount.Location = new System.Drawing.Point(200, 89);
        this.txtAccount.Name = "txtAccount";
        this.txtAccount.Size = new System.Drawing.Size(330, 23);
        this.txtAccount.TabIndex = 4;
        //
        // lblPassword
        //
        this.lblPassword.AutoSize = true;
        this.lblPassword.Location = new System.Drawing.Point(14, 128);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(62, 15);
        this.lblPassword.TabIndex = 5;
        this.lblPassword.Text = "Kennwort:";
        //
        // txtPassword
        //
        this.txtPassword.Location = new System.Drawing.Point(200, 125);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.Size = new System.Drawing.Size(330, 23);
        this.txtPassword.TabIndex = 6;
        this.txtPassword.UseSystemPasswordChar = true;
        //
        // btnInstall
        //
        this.btnInstall.Location = new System.Drawing.Point(14, 166);
        this.btnInstall.Name = "btnInstall";
        this.btnInstall.Size = new System.Drawing.Size(160, 32);
        this.btnInstall.TabIndex = 7;
        this.btnInstall.Text = "Dienst installieren";
        this.btnInstall.UseVisualStyleBackColor = true;
        this.btnInstall.Click += new System.EventHandler(this.BtnInstall_Click);
        //
        // btnSetAccount
        //
        this.btnSetAccount.Location = new System.Drawing.Point(184, 166);
        this.btnSetAccount.Name = "btnSetAccount";
        this.btnSetAccount.Size = new System.Drawing.Size(130, 32);
        this.btnSetAccount.TabIndex = 8;
        this.btnSetAccount.Text = "Konto setzen";
        this.btnSetAccount.UseVisualStyleBackColor = true;
        this.btnSetAccount.Click += new System.EventHandler(this.BtnSetAccount_Click);
        //
        // btnStart
        //
        this.btnStart.Location = new System.Drawing.Point(14, 208);
        this.btnStart.Name = "btnStart";
        this.btnStart.Size = new System.Drawing.Size(100, 32);
        this.btnStart.TabIndex = 9;
        this.btnStart.Text = "Starten";
        this.btnStart.UseVisualStyleBackColor = true;
        this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
        //
        // btnStop
        //
        this.btnStop.Location = new System.Drawing.Point(124, 208);
        this.btnStop.Name = "btnStop";
        this.btnStop.Size = new System.Drawing.Size(100, 32);
        this.btnStop.TabIndex = 10;
        this.btnStop.Text = "Stoppen";
        this.btnStop.UseVisualStyleBackColor = true;
        this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
        //
        // btnUninstall
        //
        this.btnUninstall.Location = new System.Drawing.Point(234, 208);
        this.btnUninstall.Name = "btnUninstall";
        this.btnUninstall.Size = new System.Drawing.Size(130, 32);
        this.btnUninstall.TabIndex = 11;
        this.btnUninstall.Text = "Deinstallieren";
        this.btnUninstall.UseVisualStyleBackColor = true;
        this.btnUninstall.Click += new System.EventHandler(this.BtnUninstall_Click);
        //
        // btnRefresh
        //
        this.btnRefresh.Location = new System.Drawing.Point(374, 208);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(160, 32);
        this.btnRefresh.TabIndex = 12;
        this.btnRefresh.Text = "Status aktualisieren";
        this.btnRefresh.UseVisualStyleBackColor = true;
        this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
        //
        // tabAdmin
        //
        this.tabAdmin.Controls.Add(this.btnSetAdminPw);
        this.tabAdmin.Controls.Add(this.txtAdminPw2);
        this.tabAdmin.Controls.Add(this.lblConfirm);
        this.tabAdmin.Controls.Add(this.txtAdminPw);
        this.tabAdmin.Controls.Add(this.lblNewPw);
        this.tabAdmin.Controls.Add(this.lblAdminInfo);
        this.tabAdmin.Location = new System.Drawing.Point(4, 24);
        this.tabAdmin.Name = "tabAdmin";
        this.tabAdmin.Padding = new System.Windows.Forms.Padding(14);
        this.tabAdmin.Size = new System.Drawing.Size(796, 399);
        this.tabAdmin.TabIndex = 2;
        this.tabAdmin.Text = "Admin-Kennwort";
        this.tabAdmin.UseVisualStyleBackColor = true;
        //
        // lblAdminInfo
        //
        this.lblAdminInfo.AutoSize = true;
        this.lblAdminInfo.Location = new System.Drawing.Point(14, 14);
        this.lblAdminInfo.Name = "lblAdminInfo";
        this.lblAdminInfo.Size = new System.Drawing.Size(400, 30);
        this.lblAdminInfo.TabIndex = 0;
        this.lblAdminInfo.Text = "Setzt das Administrator-Kennwort für das Web-Interface. Es wird nur als\r\nPBKDF2-H" +
            "ash in server-secrets.json geschrieben und beim Serverstart übernommen.";
        //
        // lblNewPw
        //
        this.lblNewPw.AutoSize = true;
        this.lblNewPw.Location = new System.Drawing.Point(14, 73);
        this.lblNewPw.Name = "lblNewPw";
        this.lblNewPw.Size = new System.Drawing.Size(98, 15);
        this.lblNewPw.TabIndex = 1;
        this.lblNewPw.Text = "Neues Kennwort:";
        //
        // txtAdminPw
        //
        this.txtAdminPw.Location = new System.Drawing.Point(200, 70);
        this.txtAdminPw.Name = "txtAdminPw";
        this.txtAdminPw.Size = new System.Drawing.Size(300, 23);
        this.txtAdminPw.TabIndex = 2;
        this.txtAdminPw.UseSystemPasswordChar = true;
        //
        // lblConfirm
        //
        this.lblConfirm.AutoSize = true;
        this.lblConfirm.Location = new System.Drawing.Point(14, 109);
        this.lblConfirm.Name = "lblConfirm";
        this.lblConfirm.Size = new System.Drawing.Size(70, 15);
        this.lblConfirm.TabIndex = 3;
        this.lblConfirm.Text = "Bestätigen:";
        //
        // txtAdminPw2
        //
        this.txtAdminPw2.Location = new System.Drawing.Point(200, 106);
        this.txtAdminPw2.Name = "txtAdminPw2";
        this.txtAdminPw2.Size = new System.Drawing.Size(300, 23);
        this.txtAdminPw2.TabIndex = 4;
        this.txtAdminPw2.UseSystemPasswordChar = true;
        //
        // btnSetAdminPw
        //
        this.btnSetAdminPw.Location = new System.Drawing.Point(14, 150);
        this.btnSetAdminPw.Name = "btnSetAdminPw";
        this.btnSetAdminPw.Size = new System.Drawing.Size(160, 32);
        this.btnSetAdminPw.TabIndex = 5;
        this.btnSetAdminPw.Text = "Kennwort setzen";
        this.btnSetAdminPw.UseVisualStyleBackColor = true;
        this.btnSetAdminPw.Click += new System.EventHandler(this.BtnSetAdminPw_Click);
        //
        // tabDirectory
        //
        this.tabDirectory.Controls.Add(this.txtDomain);
        this.tabDirectory.Controls.Add(this.panelDirBar);
        this.tabDirectory.Location = new System.Drawing.Point(4, 24);
        this.tabDirectory.Name = "tabDirectory";
        this.tabDirectory.Padding = new System.Windows.Forms.Padding(14);
        this.tabDirectory.Size = new System.Drawing.Size(796, 399);
        this.tabDirectory.TabIndex = 3;
        this.tabDirectory.Text = "Verzeichnis (AD)";
        this.tabDirectory.UseVisualStyleBackColor = true;
        //
        // txtDomain
        //
        this.txtDomain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtDomain.Font = new System.Drawing.Font("Consolas", 9F);
        this.txtDomain.Location = new System.Drawing.Point(14, 58);
        this.txtDomain.Multiline = true;
        this.txtDomain.Name = "txtDomain";
        this.txtDomain.ReadOnly = true;
        this.txtDomain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtDomain.Size = new System.Drawing.Size(768, 327);
        this.txtDomain.TabIndex = 1;
        //
        // panelDirBar
        //
        this.panelDirBar.Controls.Add(this.btnProbe);
        this.panelDirBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelDirBar.Location = new System.Drawing.Point(14, 14);
        this.panelDirBar.Name = "panelDirBar";
        this.panelDirBar.Size = new System.Drawing.Size(768, 44);
        this.panelDirBar.TabIndex = 0;
        //
        // btnProbe
        //
        this.btnProbe.Location = new System.Drawing.Point(0, 7);
        this.btnProbe.Name = "btnProbe";
        this.btnProbe.Size = new System.Drawing.Size(160, 30);
        this.btnProbe.TabIndex = 0;
        this.btnProbe.Text = "Domäne prüfen";
        this.btnProbe.UseVisualStyleBackColor = true;
        this.btnProbe.Click += new System.EventHandler(this.BtnProbe_Click);
        //
        // MainForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(804, 621);
        this.Controls.Add(this.tabs);
        this.Controls.Add(this.panelLog);
        this.Controls.Add(this.panelTop);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MinimumSize = new System.Drawing.Size(716, 560);
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Software Deployment – Server-Konsole";
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.panelTop.ResumeLayout(false);
        this.panelTop.PerformLayout();
        this.panelLog.ResumeLayout(false);
        this.tabs.ResumeLayout(false);
        this.tabStatus.ResumeLayout(false);
        this.panelStatusBar.ResumeLayout(false);
        this.tabService.ResumeLayout(false);
        this.tabService.PerformLayout();
        this.tabAdmin.ResumeLayout(false);
        this.tabAdmin.PerformLayout();
        this.tabDirectory.ResumeLayout(false);
        this.panelDirBar.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
