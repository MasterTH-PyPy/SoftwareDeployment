# Client (Agent)

Der Client besteht aus drei Projekten:

| Projekt | Zielframework | Zweck |
|---|---|---|
| `SoftwareDeployment.Client.Core`    | `net10.0-windows` | Gemeinsame Logik: Config/State (DPAPI-geschützter API-Key), HTTPS-API-Client, Install-Runner, Logger. |
| `SoftwareDeployment.Client.Service` | `net10.0-windows` | Windows-Dienst `SoftwareDeploymentAgent`: pollt den Server über HTTPS, lädt fällige Pakete, führt sie aus, meldet zurück. |
| `SoftwareDeployment.Client.Gui`     | `net10.0-windows` (WinForms, Designer) | Status, Paketübersicht, Protokoll und Registrierung (Server-URL + Enrollment-Key). |

## Ablauf

1. GUI starten → Reiter **Konfiguration**: Server-URL (HTTPS) und **Installations-Key** eingeben, **Registrieren**. Der API-Key wird DPAPI-verschlüsselt unter `%ProgramData%\SoftwareDeployment\Agent` abgelegt.
2. Admin gibt den Client im Server-Web-Interface frei.
3. Der Dienst pollt im konfigurierten Intervall, installiert zugewiesene Pakete und meldet Exitcode/Status zurück. GUI zeigt Status, Pakete und Protokoll (Aktualisierung alle 3 s).

## Ausführung & Parallelität

- **EXE/BAT/PowerShell** laufen parallel (begrenzt), **MSI** strikt seriell (Windows Installer erlaubt nur eine Installation gleichzeitig, globaler `_MSIExecute`-Mutex).
- Jedes Paket wird in ein temporäres Arbeitsverzeichnis geladen, per **SHA-256** geprüft, ggf. entpackt, ausgeführt und das Verzeichnis danach **gelöscht** – damit läuft das lokale Laufwerk nicht mehr voll (der ursprüngliche Grund für die Ablösung des GPO-Skripts).
- Kommunikation ausschließlich über **HTTPS** (OS-Zertifikatsprüfung), API-Key nur DPAPI-verschlüsselt.

## Noch offen

- **Client-Installer** (Inno Setup) analog zum Server: Dienst registrieren, Server-URL/Enrollment-Key abfragen, Dienstkonto mit minimalen lokalen Installationsrechten. Kommt als Nächstes.
- Optionaler „Im Windows Sandbox testen"-Lauf.
