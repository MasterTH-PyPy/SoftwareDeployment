namespace SoftwareDeployment.Server.Services;

/// <summary>
/// Holds the display name shown in the web interface (browser title + sidebar).
/// Loaded from settings at startup and refreshed whenever the organization name
/// is saved, so components can read it synchronously during render without a DB
/// hit. Later the license feature will set and lock <see cref="OrganizationName"/>.
/// </summary>
public sealed class BrandingService
{
    private volatile string _organizationName = "Software Deployment";

    /// <summary>Company/organization name shown as the primary app name.</summary>
    public string OrganizationName
    {
        get => _organizationName;
        set => _organizationName = string.IsNullOrWhiteSpace(value) ? "Software Deployment" : value.Trim();
    }

    /// <summary>Fixed product descriptor shown as a subtitle.</summary>
    public string ProductName => "Softwareverteilung";
}
