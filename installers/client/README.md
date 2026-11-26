# Client-Installer — folgt mit dem Client

Hier entsteht der Inno-Setup-Installer für den Client-Agenten. Er wird:

- Dienst + GUI nach `%ProgramFiles%\SoftwareDeployment\Agent` installieren,
- bei der Installation **Server-URL** (HTTPS) und **Enrollment-Key** abfragen,
- den Agenten als Windows-Dienst `SoftwareDeploymentAgent` registrieren,
- den Dienst mit minimalen lokalen Installationsrechten einrichten.

Analog zu `installers/build-server.ps1` wird es ein `build-client.ps1` geben.
