namespace SVPM_Starlit_Virtual_Pc_Manegement;

public static class GlobalSettings
{
    public static string? ConnectionString { get; set; }
    public const string CustomerTable = "Customers";
    public const string VirtualPcTable = "VirtualPCs";
    public const string AccountTable = "Accounts";

    public static string ConnectionListPath => System.IO.Path.Combine(GetUserProfilePath(), "sql_connections.json");

    private static string? Path { get; set; }
    private static string GetUserProfilePath()
    {
        if (OperatingSystem.IsWindows())
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Path = System.IO.Path.Combine(appDataPath, @"Starlit\SVPM-Starlit-Virtual-Pc-manegement\");
            EnsureDirectoryExists();
            return Path;
        }
        else if (OperatingSystem.IsAndroid())
        {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            EnsureDirectoryExists();
            return Path;
        }
        else
        {
            throw new PlatformNotSupportedException("Podporovány jsou pouze Windows a Android.");
        }
    }

    public static void EnsureDirectoryExists()
    {
        if (Path == null)
        {
            throw new InvalidOperationException("Path není inicializováno.");
        }
        if (!Directory.Exists(Path))
        {
            Directory.CreateDirectory(Path);
        }
    }
}