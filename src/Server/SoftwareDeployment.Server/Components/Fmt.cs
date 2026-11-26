namespace SoftwareDeployment.Server.Components;

/// <summary>
/// Display helpers. Timestamps are stored as UTC <see cref="DateTime"/> but read
/// back from SQLite with <see cref="DateTimeKind.Unspecified"/>, so we re-stamp
/// them as UTC before converting to the server's local time for display.
/// </summary>
public static class Fmt
{
    public static DateTime ToLocal(DateTime utc)
        => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();

    public static string Local(DateTime utc, string format)
        => ToLocal(utc).ToString(format);
}
