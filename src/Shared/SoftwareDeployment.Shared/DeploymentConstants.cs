namespace SoftwareDeployment.Shared;

/// <summary>Shared identifiers so server, console and client agree.</summary>
public static class DeploymentConstants
{
    /// <summary>Windows service name of the server.</summary>
    public const string ServerServiceName = "SonOGDeploymentServer";
    public const string ServerServiceDisplayName = "SonOG Software Deployment Server";

    /// <summary>Windows service name of the client agent.</summary>
    public const string ClientServiceName = "SonOGDeploymentAgent";
    public const string ClientServiceDisplayName = "SonOG Software Deployment Agent";

    public const int DefaultHttpsPort = 8443;
    public const int DefaultPollIntervalSeconds = 300;
}
