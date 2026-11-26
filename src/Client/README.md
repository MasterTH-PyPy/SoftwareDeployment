# Client (Agent) — folgt als Nächstes

Dieser Ordner nimmt die Client-Projekte auf. Die Struktur ist bereits auf die
Server-Seite und die gemeinsame `SoftwareDeployment.Shared`-Bibliothek abgestimmt.

Geplante Projekte:

| Projekt | Zielframework | Zweck |
|---|---|---|
| `SoftwareDeployment.Client.Service` | `net10.0-windows` | Windows-Dienst, pollt den Server alle *n* Minuten über HTTPS, lädt fällige Pakete, führt sie im Dienstkonto aus und meldet das Ergebnis zurück. |
| `SoftwareDeployment.Client.Gui`     | `net10.0-windows` (WinForms) | Übersicht der zugewiesenen/auszuführenden Pakete, Logging, Status der letzten Läufe. |

Bereits serverseitig vorbereitet und vom Client zu nutzen:

- **Vertrag**: `SoftwareDeployment.Shared.Contracts` (Registrierung, Poll, Report)
  und `SoftwareDeployment.Shared.ApiRoutes`.
- **Registrierung**: `POST /api/v1/clients/register` mit **Enrollment-Key**
  (aus dem Server-Interface) → liefert `ClientId` + `ApiKey` (einmalig).
  Danach Status `Pending`, bis der Admin den Client freigibt.
- **Poll**: `POST /api/v1/clients/poll` mit `X-Client-Id` / `X-Api-Key`.
  Der Client meldet den angemeldeten Benutzer und seine installierten
  Versionen; der Server liefert nur die Pakete zurück, deren aktuelle Version
  abweicht.
- **Download**: `GET /api/v1/packages/{id}/versions/{version}/payload`
  (SHA-256 zur Verifikation steht in der Poll-Antwort).
- **Report**: `POST /api/v1/clients/report` mit Exitcode/Status je Paket.

Sicherheitsvorgaben, die der Client einhalten muss:

- **Nur HTTPS**, Server-Zertifikat validieren (keine unverschlüsselte Kommunikation).
- API-Key **DPAPI-verschlüsselt** ablegen (`DataProtectionScope.LocalMachine`),
  keine Klartext-Konfiguration.
- Ausführung mit **minimalen lokalen Installationsrechten** im Dienstkonto.
