; Inno Setup script for the Software Deployment CLIENT (agent + GUI).
; Build with installers/build-client.ps1 (publishes the binaries first), or:
;   ISCC.exe /DSourceDir="..\..\staging\client" SoftwareDeployment.Client.iss

#define AppName "Software Deployment Agent"
#define AppVersion "0.1.0"
#define Publisher "Software Deployment"
#define ServiceName "SoftwareDeploymentAgent"
#define ServiceExe "SoftwareDeployment.Agent.exe"
#define GuiExe "SoftwareDeployment.Agent.Gui.exe"

#ifndef SourceDir
  #define SourceDir "..\..\staging\client"
#endif

[Setup]
AppId={{A7C1F0E3-5B22-4A91-9E7C-2D0B3F6A1E02}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\SoftwareDeployment\Agent
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputDir=Output
OutputBaseFilename=SoftwareDeployment-Agent-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64compatible
WizardStyle=modern
UninstallDisplayName={#AppName}

[Languages]
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\Agent-GUI"; Filename: "{app}\{#GuiExe}"
Name: "{group}\{#AppName} deinstallieren"; Filename: "{uninstallexe}"

[Run]
; Register the service as LocalSystem (local install rights, no domain account).
Filename: "{sys}\sc.exe"; \
  Parameters: "create {#ServiceName} binPath= ""{app}\{#ServiceExe}"" start= auto obj= LocalSystem DisplayName= ""{#AppName}"""; \
  Flags: runhidden; StatusMsg: "Dienst wird registriert..."
Filename: "{sys}\sc.exe"; \
  Parameters: "description {#ServiceName} ""Software Deployment Agent - holt und installiert zugewiesene Pakete."""; \
  Flags: runhidden
; Register with the server (enrollment key passed only as an argument, never stored in clear text).
Filename: "{app}\{#ServiceExe}"; \
  Parameters: "register --server ""{code:GetServerUrl}"" --key ""{code:GetEnrollmentKey}"""; \
  Flags: runhidden; StatusMsg: "Registrierung am Server..."
; Start the service.
Filename: "{sys}\sc.exe"; Parameters: "start {#ServiceName}"; Flags: runhidden; StatusMsg: "Dienst wird gestartet..."
; Offer to open the GUI.
Filename: "{app}\{#GuiExe}"; Description: "Agent-GUI jetzt oeffnen"; Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{sys}\sc.exe"; Parameters: "stop {#ServiceName}"; Flags: runhidden; RunOnceId: "StopSvc"
Filename: "{sys}\sc.exe"; Parameters: "delete {#ServiceName}"; Flags: runhidden; RunOnceId: "DelSvc"

[UninstallDelete]
Type: filesandordirs; Name: "{commonappdata}\SoftwareDeployment\Agent"

[Code]
var
  AgentPage: TInputQueryWizardPage;

procedure InitializeWizard;
begin
  AgentPage := CreateInputQueryPage(wpSelectDir,
    'Server-Verbindung',
    'Verbindung zum Deployment-Server',
    'Bitte die Server-URL (HTTPS) und den Installations-Key eingeben. Der Key steht im Server-Web-Interface unter Einstellungen.');
  AgentPage.Add('Server-URL (HTTPS):', False);
  AgentPage.Add('Installations-Key:', False);
  AgentPage.Values[0] := 'https://';
end;

function GetServerUrl(Param: String): String;
begin
  Result := Trim(AgentPage.Values[0]);
end;

function GetEnrollmentKey(Param: String): String;
begin
  Result := Trim(AgentPage.Values[1]);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = AgentPage.ID then
  begin
    if Pos('https://', LowerCase(Trim(AgentPage.Values[0]))) <> 1 then
    begin
      MsgBox('Die Server-URL muss mit https:// beginnen (keine unverschluesselte Verbindung).', mbError, MB_OK);
      Result := False;
    end
    else if Trim(AgentPage.Values[1]) = '' then
    begin
      MsgBox('Bitte den Installations-Key eingeben.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;
