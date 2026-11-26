using System.DirectoryServices;
using System.Runtime.Versioning;

namespace SoftwareDeployment.Server.Console;

[SupportedOSPlatform("windows")]
public static class DomainProbe
{
    public static string Probe()
    {
        try
        {
            using var rootDse = new DirectoryEntry("LDAP://RootDSE");
            string? nc = rootDse.Properties["defaultNamingContext"].Value?.ToString();
            string? dc = rootDse.Properties["dnsHostName"].Value?.ToString();
            string? forest = rootDse.Properties["rootDomainNamingContext"].Value?.ToString();

            if (string.IsNullOrEmpty(nc))
                return "Keine Domäne erkannt (RootDSE leer). Läuft der Rechner domänengebunden?";

            return $"""
                Verbunden mit Active Directory:
                  Domäne (defaultNamingContext): {nc}
                  Gesamtstruktur (root):          {forest}
                  Domänencontroller:              {dc}

                Die Zuweisung von Paketen zu Computern, Gruppen und Benutzern
                erfolgt im Web-Interface unter „Pakete“ → Paket öffnen → Zuweisungen.
                """;
        }
        catch (Exception ex)
        {
            return "Fehler beim Verbinden mit Active Directory: " + ex.Message;
        }
    }
}
