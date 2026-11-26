using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SoftwareDeployment.Client.Core;

/// <summary>Identity details the agent reports to the server.</summary>
public static class ClientInfo
{
    public static string MachineName => Environment.MachineName;

    public static string Domain => Environment.UserDomainName;

    public static string OsVersion => RuntimeInformation.OSDescription;

    public static string AgentVersion =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "0.0.0";

    /// <summary>
    /// SAM account name of the user on the active console session (for user/group
    /// assignments). The service runs in session 0, so we query WTS. Null if nobody
    /// is logged on or it cannot be determined.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static string? GetConsoleUser()
    {
        try
        {
            uint sessionId = WTSGetActiveConsoleSessionId();
            if (sessionId == 0xFFFFFFFF) return null;

            string? user = QuerySession(sessionId, WtsInfoClass.WTSUserName);
            if (string.IsNullOrEmpty(user)) return null;
            return user;
        }
        catch
        {
            return null;
        }
    }

    [SupportedOSPlatform("windows")]
    private static string? QuerySession(uint sessionId, WtsInfoClass info)
    {
        if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, info, out IntPtr buffer, out _))
            return null;
        try
        {
            string? value = Marshal.PtrToStringUni(buffer);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        finally
        {
            WTSFreeMemory(buffer);
        }
    }

    private enum WtsInfoClass
    {
        WTSUserName = 5,
        WTSDomainName = 7,
    }

    [DllImport("kernel32.dll")]
    private static extern uint WTSGetActiveConsoleSessionId();

    [DllImport("wtsapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool WTSQuerySessionInformation(
        IntPtr server, uint sessionId, WtsInfoClass infoClass, out IntPtr buffer, out int bytesReturned);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr memory);
}
