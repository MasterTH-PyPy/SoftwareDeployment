# Client-Installer

Inno-Setup-Installer für den Client-Agenten (`SoftwareDeployment.Client.iss`).

Er installiert Dienst + GUI nach `%ProgramFiles%\SoftwareDeployment\Agent`, fragt
während der Installation **Server-URL (HTTPS)** und **Installations-Key** ab,
registriert den Windows-Dienst `SoftwareDeploymentAgent` (Konto **LocalSystem** =
lokale Installationsrechte, kein Domänenkonto), ruft die Registrierung am Server
auf (`SoftwareDeployment.Agent.exe register --server … --key …` – der Key wird nur
als Argument übergeben, nicht in Klartext gespeichert) und startet den Dienst.

## Build

```powershell
# framework-abhängig (benötigt .NET-10-Runtime auf dem Client)
installers/build-client.ps1

# oder self-contained (Runtime gebündelt)
installers/build-client.ps1 -SelfContained
```

Voraussetzung: Inno Setup 6 (`ISCC.exe`). Das Setup landet in
`installers/client/Output`.

## Danach

Der Client erscheint im Server-Web-Interface unter *Clients* als „Freigabe
offen". Nach der Freigabe beginnt der Dienst beim nächsten Poll mit der
Installation zugewiesener Pakete. Status/Protokoll zeigt die Agent-GUI.
