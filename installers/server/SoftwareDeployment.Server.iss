; Inno Setup script for the Software Deployment SERVER.
; Build with installers/build-server.ps1 (it publishes the binaries first and
; passes SourceDir), or compile directly with:
;   ISCC.exe /DSourceDir="..\..\staging\server" SoftwareDeployment.Server.iss

#define AppName "Software Deployment Server"
#define AppVersion "0.1.0"
#define Publisher "Software Deployment"
#define ServiceExe "SoftwareDeployment.Server.exe"
#define ConsoleExe "SoftwareDeployment.Server.Console.exe"

#ifndef SourceDir
  #define SourceDir "..\..\staging\server"
#endif

[Setup]
AppId={{B3E4B1A2-7C21-4E9A-9E2B-7D0C3F5A1E01}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\SoftwareDeployment\Server
DefaultGroupName=Software Deployment
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputDir=Output
OutputBaseFilename=SoftwareDeployment-Server-Setup-{#AppVersion}
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

[Dirs]
; Runtime state. Restrict to Administrators + SYSTEM + the service account.
Name: "{app}\data"; Permissions: admins-full system-full

[Icons]
Name: "{group}\Server-Konsole"; Filename: "{app}\{#ConsoleExe}"
Name: "{group}\Web-Interface (HTTPS)"; Filename: "https://localhost:8443/"
Name: "{group}\{#AppName} deinstallieren"; Filename: "{uninstallexe}"

[Run]
; Allow inbound HTTPS to the deployment API / web interface.
Filename: "{sys}\netsh.exe"; \
  Parameters: "advfirewall firewall add rule name=""Software Deployment HTTPS"" dir=in action=allow protocol=TCP localport=8443"; \
  Flags: runhidden; StatusMsg: "Firewall-Regel fuer HTTPS (8443) wird angelegt..."
; Open the console so the admin can set the account, password and install the service.
Filename: "{app}\{#ConsoleExe}"; \
  Description: "Server-Konsole oeffnen (Dienstkonto, Kennwort, Dienst einrichten)"; \
  Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{sys}\sc.exe"; Parameters: "stop SoftwareDeploymentServer"; Flags: runhidden; RunOnceId: "StopSvc"
Filename: "{sys}\sc.exe"; Parameters: "delete SoftwareDeploymentServer"; Flags: runhidden; RunOnceId: "DelSvc"
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""Software Deployment HTTPS"""; Flags: runhidden; RunOnceId: "DelFw"

[UninstallDelete]
; Leave data by default? We remove it; comment out to preserve the database.
Type: filesandordirs; Name: "{app}\data"
