namespace SoftwareDeployment.Client.Gui;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.TabControl tabs;
    private System.Windows.Forms.TabPage tabStatus;
    private System.Windows.Forms.TabPage tabPackages;
    private System.Windows.Forms.TabPage tabLog;
    private System.Windows.Forms.TabPage tabConfig;

    private System.Windows.Forms.Panel panelStatusBar;
    private System.Windows.Forms.Button btnStatusRefresh;
    private System.Windows.Forms.TextBox txtStatus;

    private System.Windows.Forms.ListView lvPackages;
    private System.Windows.Forms.ColumnHeader colPkgName;
    private System.Windows.Forms.ColumnHeader colPkgVersion;
    private System.Windows.Forms.ColumnHeader colPkgStatus;
    private System.Windows.Forms.ColumnHeader colPkgUpdated;
    private System.Windows.Forms.ColumnHeader colPkgMsg;

    private System.Windows.Forms.Panel panelLogBar;
    private System.Windows.Forms.Button btnLogRefresh;
    private System.Windows.Forms.TextBox txtLog;

    private System.Windows.Forms.Label lblServer;
    private System.Windows.Forms.TextBox txtServer;
    private System.Windows.Forms.Label lblKey;
    private System.Windows.Forms.TextBox txtKey;
    private System.Windows.Forms.Button btnRegister;
    private System.Windows.Forms.Label lblRegResult;
    private System.Windows.Forms.Label lblConfigInfo;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.tabs = new System.Windows.Forms.TabControl();
        this.tabStatus = new System.Windows.Forms.TabPage();
        this.txtStatus = new System.Windows.Forms.TextBox();
        this.panelStatusBar = new System.Windows.Forms.Panel();
        this.btnStatusRefresh = new System.Windows.Forms.Button();
        this.tabPackages = new System.Windows.Forms.TabPage();
        this.lvPackages = new System.Windows.Forms.ListView();
        this.colPkgName = new System.Windows.Forms.ColumnHeader();
        this.colPkgVersion = new System.Windows.Forms.ColumnHeader();
        this.colPkgStatus = new System.Windows.Forms.ColumnHeader();
        this.colPkgUpdated = new System.Windows.Forms.ColumnHeader();
        this.colPkgMsg = new System.Windows.Forms.ColumnHeader();
        this.tabLog = new System.Windows.Forms.TabPage();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.panelLogBar = new System.Windows.Forms.Panel();
        this.btnLogRefresh = new System.Windows.Forms.Button();
        this.tabConfig = new System.Windows.Forms.TabPage();
        this.lblServer = new System.Windows.Forms.Label();
        this.txtServer = new System.Windows.Forms.TextBox();
        this.lblKey = new System.Windows.Forms.Label();
        this.txtKey = new System.Windows.Forms.TextBox();
        this.btnRegister = new System.Windows.Forms.Button();
        this.lblRegResult = new System.Windows.Forms.Label();
        this.lblConfigInfo = new System.Windows.Forms.Label();
        this.tabs.SuspendLayout();
        this.tabStatus.SuspendLayout();
        this.panelStatusBar.SuspendLayout();
        this.tabPackages.SuspendLayout();
        this.tabLog.SuspendLayout();
        this.panelLogBar.SuspendLayout();
        this.tabConfig.SuspendLayout();
        this.SuspendLayout();
        //
        // tabs
        //
        this.tabs.Controls.Add(this.tabStatus);
        this.tabs.Controls.Add(this.tabPackages);
        this.tabs.Controls.Add(this.tabLog);
        this.tabs.Controls.Add(this.tabConfig);
        this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabs.Location = new System.Drawing.Point(0, 0);
        this.tabs.Name = "tabs";
        this.tabs.SelectedIndex = 0;
        this.tabs.Size = new System.Drawing.Size(820, 600);
        this.tabs.TabIndex = 0;
        //
        // tabStatus
        //
        this.tabStatus.Controls.Add(this.txtStatus);
        this.tabStatus.Controls.Add(this.panelStatusBar);
        this.tabStatus.Location = new System.Drawing.Point(4, 24);
        this.tabStatus.Name = "tabStatus";
        this.tabStatus.Padding = new System.Windows.Forms.Padding(10);
        this.tabStatus.Size = new System.Drawing.Size(812, 572);
        this.tabStatus.TabIndex = 0;
        this.tabStatus.Text = "Status";
        this.tabStatus.UseVisualStyleBackColor = true;
        //
        // txtStatus
        //
        this.txtStatus.BackColor = System.Drawing.Color.White;
        this.txtStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtStatus.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.txtStatus.Location = new System.Drawing.Point(10, 54);
        this.txtStatus.Multiline = true;
        this.txtStatus.Name = "txtStatus";
        this.txtStatus.ReadOnly = true;
        this.txtStatus.Size = new System.Drawing.Size(792, 508);
        this.txtStatus.TabIndex = 1;
        //
        // panelStatusBar
        //
        this.panelStatusBar.Controls.Add(this.btnStatusRefresh);
        this.panelStatusBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelStatusBar.Location = new System.Drawing.Point(10, 10);
        this.panelStatusBar.Name = "panelStatusBar";
        this.panelStatusBar.Size = new System.Drawing.Size(792, 44);
        this.panelStatusBar.TabIndex = 0;
        //
        // btnStatusRefresh
        //
        this.btnStatusRefresh.Location = new System.Drawing.Point(0, 7);
        this.btnStatusRefresh.Name = "btnStatusRefresh";
        this.btnStatusRefresh.Size = new System.Drawing.Size(160, 30);
        this.btnStatusRefresh.TabIndex = 0;
        this.btnStatusRefresh.Text = "Aktualisieren";
        this.btnStatusRefresh.UseVisualStyleBackColor = true;
        this.btnStatusRefresh.Click += new System.EventHandler(this.BtnStatusRefresh_Click);
        //
        // tabPackages
        //
        this.tabPackages.Controls.Add(this.lvPackages);
        this.tabPackages.Location = new System.Drawing.Point(4, 24);
        this.tabPackages.Name = "tabPackages";
        this.tabPackages.Padding = new System.Windows.Forms.Padding(10);
        this.tabPackages.Size = new System.Drawing.Size(812, 572);
        this.tabPackages.TabIndex = 1;
        this.tabPackages.Text = "Pakete";
        this.tabPackages.UseVisualStyleBackColor = true;
        //
        // lvPackages
        //
        this.lvPackages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPkgName,
            this.colPkgVersion,
            this.colPkgStatus,
            this.colPkgUpdated,
            this.colPkgMsg});
        this.lvPackages.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lvPackages.FullRowSelect = true;
        this.lvPackages.GridLines = true;
        this.lvPackages.Location = new System.Drawing.Point(10, 10);
        this.lvPackages.Name = "lvPackages";
        this.lvPackages.Size = new System.Drawing.Size(792, 552);
        this.lvPackages.TabIndex = 0;
        this.lvPackages.UseCompatibleStateImageBehavior = false;
        this.lvPackages.View = System.Windows.Forms.View.Details;
        //
        // columns
        //
        this.colPkgName.Text = "Paket";
        this.colPkgName.Width = 220;
        this.colPkgVersion.Text = "Version";
        this.colPkgVersion.Width = 110;
        this.colPkgStatus.Text = "Status";
        this.colPkgStatus.Width = 110;
        this.colPkgUpdated.Text = "Aktualisiert";
        this.colPkgUpdated.Width = 140;
        this.colPkgMsg.Text = "Meldung";
        this.colPkgMsg.Width = 200;
        //
        // tabLog
        //
        this.tabLog.Controls.Add(this.txtLog);
        this.tabLog.Controls.Add(this.panelLogBar);
        this.tabLog.Location = new System.Drawing.Point(4, 24);
        this.tabLog.Name = "tabLog";
        this.tabLog.Padding = new System.Windows.Forms.Padding(10);
        this.tabLog.Size = new System.Drawing.Size(812, 572);
        this.tabLog.TabIndex = 2;
        this.tabLog.Text = "Protokoll";
        this.tabLog.UseVisualStyleBackColor = true;
        //
        // txtLog
        //
        this.txtLog.BackColor = System.Drawing.Color.White;
        this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtLog.Font = new System.Drawing.Font("Consolas", 8.75F);
        this.txtLog.Location = new System.Drawing.Point(10, 54);
        this.txtLog.Multiline = true;
        this.txtLog.Name = "txtLog";
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.txtLog.Size = new System.Drawing.Size(792, 508);
        this.txtLog.TabIndex = 1;
        this.txtLog.WordWrap = false;
        //
        // panelLogBar
        //
        this.panelLogBar.Controls.Add(this.btnLogRefresh);
        this.panelLogBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelLogBar.Location = new System.Drawing.Point(10, 10);
        this.panelLogBar.Name = "panelLogBar";
        this.panelLogBar.Size = new System.Drawing.Size(792, 44);
        this.panelLogBar.TabIndex = 0;
        //
        // btnLogRefresh
        //
        this.btnLogRefresh.Location = new System.Drawing.Point(0, 7);
        this.btnLogRefresh.Name = "btnLogRefresh";
        this.btnLogRefresh.Size = new System.Drawing.Size(160, 30);
        this.btnLogRefresh.TabIndex = 0;
        this.btnLogRefresh.Text = "Protokoll aktualisieren";
        this.btnLogRefresh.UseVisualStyleBackColor = true;
        this.btnLogRefresh.Click += new System.EventHandler(this.BtnLogRefresh_Click);
        //
        // tabConfig
        //
        this.tabConfig.Controls.Add(this.lblConfigInfo);
        this.tabConfig.Controls.Add(this.lblRegResult);
        this.tabConfig.Controls.Add(this.btnRegister);
        this.tabConfig.Controls.Add(this.txtKey);
        this.tabConfig.Controls.Add(this.lblKey);
        this.tabConfig.Controls.Add(this.txtServer);
        this.tabConfig.Controls.Add(this.lblServer);
        this.tabConfig.Location = new System.Drawing.Point(4, 24);
        this.tabConfig.Name = "tabConfig";
        this.tabConfig.Padding = new System.Windows.Forms.Padding(14);
        this.tabConfig.Size = new System.Drawing.Size(812, 572);
        this.tabConfig.TabIndex = 3;
        this.tabConfig.Text = "Konfiguration";
        this.tabConfig.UseVisualStyleBackColor = true;
        //
        // lblServer
        //
        this.lblServer.AutoSize = true;
        this.lblServer.Location = new System.Drawing.Point(14, 21);
        this.lblServer.Name = "lblServer";
        this.lblServer.Size = new System.Drawing.Size(120, 15);
        this.lblServer.TabIndex = 0;
        this.lblServer.Text = "Server-URL (HTTPS):";
        //
        // txtServer
        //
        this.txtServer.Location = new System.Drawing.Point(200, 18);
        this.txtServer.Name = "txtServer";
        this.txtServer.PlaceholderText = "https://deployment.example.local:8443";
        this.txtServer.Size = new System.Drawing.Size(440, 23);
        this.txtServer.TabIndex = 1;
        //
        // lblKey
        //
        this.lblKey.AutoSize = true;
        this.lblKey.Location = new System.Drawing.Point(14, 57);
        this.lblKey.Name = "lblKey";
        this.lblKey.Size = new System.Drawing.Size(103, 15);
        this.lblKey.TabIndex = 2;
        this.lblKey.Text = "Installations-Key:";
        //
        // txtKey
        //
        this.txtKey.Location = new System.Drawing.Point(200, 54);
        this.txtKey.Name = "txtKey";
        this.txtKey.Size = new System.Drawing.Size(440, 23);
        this.txtKey.TabIndex = 3;
        //
        // btnRegister
        //
        this.btnRegister.Location = new System.Drawing.Point(200, 90);
        this.btnRegister.Name = "btnRegister";
        this.btnRegister.Size = new System.Drawing.Size(160, 32);
        this.btnRegister.TabIndex = 4;
        this.btnRegister.Text = "Registrieren";
        this.btnRegister.UseVisualStyleBackColor = true;
        this.btnRegister.Click += new System.EventHandler(this.BtnRegister_Click);
        //
        // lblRegResult
        //
        this.lblRegResult.AutoSize = true;
        this.lblRegResult.Location = new System.Drawing.Point(14, 140);
        this.lblRegResult.Name = "lblRegResult";
        this.lblRegResult.Size = new System.Drawing.Size(0, 15);
        this.lblRegResult.TabIndex = 5;
        //
        // lblConfigInfo
        //
        this.lblConfigInfo.AutoSize = true;
        this.lblConfigInfo.Location = new System.Drawing.Point(14, 180);
        this.lblConfigInfo.Name = "lblConfigInfo";
        this.lblConfigInfo.Size = new System.Drawing.Size(0, 15);
        this.lblConfigInfo.TabIndex = 6;
        //
        // MainForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(820, 600);
        this.Controls.Add(this.tabs);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.MinimumSize = new System.Drawing.Size(700, 520);
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "SonOG Deployment Agent";
        this.tabs.ResumeLayout(false);
        this.tabStatus.ResumeLayout(false);
        this.tabStatus.PerformLayout();
        this.panelStatusBar.ResumeLayout(false);
        this.tabPackages.ResumeLayout(false);
        this.tabLog.ResumeLayout(false);
        this.tabLog.PerformLayout();
        this.panelLogBar.ResumeLayout(false);
        this.tabConfig.ResumeLayout(false);
        this.tabConfig.PerformLayout();
        this.ResumeLayout(false);
    }
}
