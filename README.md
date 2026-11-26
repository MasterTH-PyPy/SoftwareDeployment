# SonOG Software Deployment

Zentrale Softwareverteilung für Windows-/AD-Umgebungen als Client-/Server-Lösung
(.NET 10). Ablösung eines GPO-Logon-Skripts durch einen pollenden Client-Dienst
und einen zentral administrierbaren Server mit Web-Interface.

> **Status:** Server + Server-Konsole + Server-Installer stehen. Der Client
> (Dienst + GUI) und sein Installer folgen als Nächstes. Die Git-Struktur ist
> bereits darauf ausgelegt.

## Highlights

- **Pull statt Push**: Client-Dienst fragt den Server alle *n* Minuten (Standard 5).
- **Nur was fehlt**: serverseitiger Versionsabgleich – kein unnötiger Download,
  keine volllaufenden Client-Laufwerke.
- **Web-Interface**: Pakete hochladen, Versionen/Zuweisungen pflegen, Clients
  freigeben, Einstellungen – inkl. Logo.
- **AD-integriert**: Zuweisung an Computerkonten, Gruppen und Benutzer (per SID,
  inkl. rekursiver Gruppen).
- **Sicher by default**: ausschließlich HTTPS, Enrollment-Key bei der
  Installation, Client-Freigabe, Geheimnisse nur gehasht/verschlüsselt.
- **Installer-Typen**: EXE, MSI (msiexec), Batch, PowerShell – mit Argumenten und
  Erfolgs-Exitcodes.

## Struktur

```
SoftwareDeployment.sln
src/
  Shared/   SoftwareDeployment.Shared           Vertrag (DTOs, Enums, Routen)
  Server/   SoftwareDeployment.Server            ASP.NET Core 10: Client-API + Blazor-Web-Interface
            SoftwareDeployment.Server.Console    WinForms: Dienst/Kennwort/Abhängigkeiten/AD-Test
  Client/   (folgt)
installers/ server/  Inno-Setup + build-server.ps1
docs/       architecture.md
```

Details: siehe [docs/architecture.md](docs/architecture.md).

## Voraussetzungen

- .NET 10 SDK (Server + Konsole bauen)
- Für den produktiven Betrieb: domänengebundener Windows-Host, TLS-Zertifikat,
  AD-Lesekonto für den Dienst
- Für den Installer: Inno Setup 6 (`ISCC.exe`)

## Entwicklung

```powershell
# einmalig: Entwickler-Zertifikat fuer HTTPS
dotnet dev-certs https --trust

# Server starten (Development: https://localhost:7443)
dotnet run --project src/Server/SoftwareDeployment.Server

# Konsole starten (Windows)
dotnet run --project src/Server/SoftwareDeployment.Server.Console
```

Beim ersten Aufruf des Web-Interfaces wird ein Administrator-Kennwort gesetzt
(oder vorab über die Server-Konsole). Den **Enrollment-Key** zeigt das
Web-Interface unter *Einstellungen*.

## Build des Server-Installers

```powershell
# framework-abhaengig (benoetigt .NET-10-Runtime auf dem Zielserver)
installers/build-server.ps1

# oder self-contained (Runtime gebuendelt)
installers/build-server.ps1 -SelfContained
```

Das Setup installiert Server + Konsole, legt eine Firewall-Regel für HTTPS (8443)
an und öffnet die Konsole zur Einrichtung von Dienstkonto, Kennwort und Dienst.

## Produktiv-Konfiguration (Kurzfassung)

1. Setup ausführen.
2. **TLS-Zertifikat** hinterlegen – entweder per `appsettings.json`
   (`Kestrel:Certificates:Default` mit Pfad/Passwort) oder über den
   Windows-Zertifikatspeicher. Der Server erzwingt HTTPS.
3. In der **Server-Konsole**: Dienstkonto wählen (Domänenkonto/gMSA),
   Admin-Kennwort setzen, Abhängigkeiten prüfen, Dienst installieren & starten.
4. Im **Web-Interface**: Pakete anlegen/hochladen, Zuweisungen setzen, Clients
   freigeben. Enrollment-Key notieren für die Client-Installation.

## Lizenz / Eigentum

Internes Projekt der SonOG. Logo unter
`src/Server/SoftwareDeployment.Server/wwwroot/img/logo.svg` – bei Bedarf durch das
offizielle SonOG-Logo ersetzen.
