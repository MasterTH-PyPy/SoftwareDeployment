# Architektur

## Überblick

Ablösung des bisherigen GPO-Logon-Skripts durch ein **Pull-basiertes
Client-/Server-Modell**:

- Der **Client-Agent** läuft als Windows-Dienst und fragt den Server in einem
  konfigurierbaren Intervall (Standard 5 min) über HTTPS: „Gibt es etwas zu
  tun?“. Fällige Pakete werden heruntergeladen, lokal ausgeführt und das
  Ergebnis zurückgemeldet.
- Der **Server** stellt die Client-API bereit, hält Pakete, Zuweisungen und
  Protokolle und wird über ein **Web-Interface** administriert. Eine
  **WinForms-Server-Konsole** übernimmt Einrichtung (Dienstkonto, Kennwort,
  Abhängigkeiten).

Gegenüber dem alten Skript gelöst:

- **Kein Volllaufen lokaler Laufwerke**: der Client lädt nur, was wirklich fehlt
  (Versionsabgleich serverseitig), und räumt temporäre Nutzdaten nach der
  Installation wieder ab (Client-Logik folgt mit dem Agenten).
- **Zentrale Verwaltung** statt Dateiablage mit `install.command` /
  `computerinclude` / `groupinclude`.
- **Freigabe** jedes Clients und **Enrollment-Key** gegen unkontrollierte
  Anmeldung.

## Komponenten

```
            HTTPS (nur verschlüsselt)
Client-Agent  ───────────────────────────►  Server (ASP.NET Core 10)
(Windows-Dienst)   register / poll /          ├─ Client-API  /api/v1/*
                   download / report          ├─ Web-Interface (Blazor Server)
                                              └─ SQLite + Paketspeicher (Disk)
Server-Konsole (WinForms) ──► Dienst/Setup/Kennwort/AD-Test
```

## Projektstruktur

```
src/
  Shared/   SoftwareDeployment.Shared          Vertrag: DTOs, Enums, Routen, Konstanten
  Server/   SoftwareDeployment.Server          ASP.NET Core: API + Blazor-Web-Interface
            SoftwareDeployment.Server.Console   WinForms: Setup/Dienst/Kennwort/AD-Test
  Client/   (folgt)                             Dienst + GUI
installers/ server/  Inno-Setup + build-server.ps1
            client/  (folgt)
```

## Ablauf (Deployment)

1. **Registrierung**: Client sendet Maschinenname, Domäne, Agent-Version und den
   **Enrollment-Key**. Bei korrektem Key legt der Server einen Client-Datensatz
   an (Status `Pending`) und gibt `ClientId` + `ApiKey` (einmalig) zurück.
2. **Freigabe**: Der Admin gibt den Client im Web-Interface frei (`Approved`).
3. **Poll**: Der Client meldet den angemeldeten Benutzer und seine installierten
   Versionen. Der Server ermittelt über AD alle wirksamen SIDs (Computerkonto +
   Benutzer + jeweils rekursive Gruppenmitgliedschaften), gleicht sie gegen die
   Zuweisungen ab und liefert die Pakete zurück, deren **aktuelle** Version von
   der installierten abweicht.
4. **Download**: Der Client lädt die Nutzdaten und verifiziert sie per SHA-256.
5. **Ausführung**: Lokale Ausführung je nach Installer-Typ (EXE/MSI/BAT/PS) mit
   definierten Erfolgs-Exitcodes (z. B. `0,3010`).
6. **Report**: Ergebnis (Status/Exitcode/Meldung) geht zurück; der Server führt
   Protokoll und aktualisiert den Installationsstand je Client.

## Datenmodell (SQLite)

- `Clients` — Maschine, Domäne, Status, API-Key-Hash, zuletzt gesehen, Benutzer.
- `Packages` / `PackageVersions` — Paket + Versionen; genau eine Version je Paket
  ist „aktuell“. Nutzdaten liegen unter GUID-Namen auf der Platte; die DB hält
  nur Metadaten + SHA-256.
- `Assignments` — Paket ↔ AD-Prinzipal (Computer/Gruppe/Benutzer) **per SID**.
- `ClientPackageStates` — aktueller Installationsstand je Client/Paket.
- `InstallLogs` — Historie der Rückmeldungen.
- `Settings` — Poll-Intervall, Auto-Freigabe, Organisation, Admin-Kennwort-Hash,
  DPAPI-geschützter Enrollment-Key.

## Sicherheit

- **Transport**: ausschließlich HTTPS (HTTPS-Redirect, HSTS, `Secure`-Cookies,
  `SameSite=Strict`). Kein unverschlüsselter Endpunkt für Inhalte.
- **Enrollment-Key**: erforderlich bei der Client-Installation; verschlüsselt
  (DPAPI, LocalMachine) gespeichert, im Web-Interface auslesbar/erneuerbar.
- **Freigabe**: jeder Client muss nach der Registrierung manuell freigegeben
  werden (Auto-Freigabe optional, nicht empfohlen).
- **Geheimnisse at rest**: API-Keys und Admin-Kennwort nur als PBKDF2-Hash in der
  DB; der Bootstrap (`server-secrets.json`) enthält ausschließlich den
  Kennwort-Hash. Enrollment-Key DPAPI-verschlüsselt. Keine Klartext-Konfiguration.
- **Download-Autorisierung**: auch heruntergeladen wird nur, was dem Client
  tatsächlich zugewiesen ist.
- **Least Privilege**: Server-Dienstkonto benötigt nur AD-Leserechte; der
  Client-Agent führt mit minimalen lokalen Installationsrechten aus.

## Bekannte Grenzen / nächste Schritte

- Schema wird aktuell per `EnsureCreated()` erzeugt; für spätere Änderungen auf
  EF-Core-Migrationen umstellen.
- Große Nutzdaten laufen beim Upload derzeit über den Blazor-Circuit; später
  besser ein dedizierter gechunkter Upload-Endpunkt.
- Re-Registrierung erfolgt per Upsert über Maschinenname+Domäne; für höhere
  Sicherheit später Maschinenzertifikate (mTLS) erwägen.
- Client (Dienst + GUI) und Client-Installer folgen als Nächstes.
