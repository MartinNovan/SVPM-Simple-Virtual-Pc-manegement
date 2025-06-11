namespace SVPM;

public static class GlobalSettings
{
    public static string? ConnectionString { get; set; }
    public static readonly string ConnectionListPath = FileHelpers.GetUserProfilePath() + "sql_connections.json";
    public static string Version { get; } = "1.0.0.0";
}