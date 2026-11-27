using Microsoft.AspNetCore.Components;
using SoftwareDeployment.Shared;

namespace SoftwareDeployment.Server.Components;

/// <summary>Renders coloured status pills. All inputs are fixed enum labels, never user text.</summary>
public static class StatusBadge
{
    public static MarkupString Render(ClientApprovalStatus status) => status switch
    {
        ClientApprovalStatus.Approved => Pill("badge-approved", "Freigegeben"),
        ClientApprovalStatus.Rejected => Pill("badge-rejected", "Abgelehnt"),
        _ => Pill("badge-pending", "Offen"),
    };

    public static MarkupString Render(InstallResultStatus status) => status switch
    {
        InstallResultStatus.Success => Pill("badge-success", "Erfolg"),
        InstallResultStatus.Skipped => Pill("badge-muted", "Übersprungen"),
        InstallResultStatus.Failed => Pill("badge-failed", "Fehler"),
        InstallResultStatus.Running => Pill("badge-running", "Läuft"),
        _ => Pill("badge-muted", "Ausstehend"),
    };

    public static MarkupString RenderEnabled() => Pill("badge-approved", "Aktiv");
    public static MarkupString RenderDisabled() => Pill("badge-muted", "Deaktiviert");

    /// <summary>"Aktiv"-Badge wenn aktuell, sonst leer – immer MarkupString, damit
    /// Razor es als HTML rendert statt es zu encodieren.</summary>
    public static MarkupString Current(bool isCurrent)
        => isCurrent ? RenderEnabled() : new MarkupString(string.Empty);

    private static MarkupString Pill(string cls, string text)
        => new($"<span class=\"badge {cls}\">{text}</span>");
}
