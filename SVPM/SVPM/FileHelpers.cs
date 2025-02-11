using System.Text.Json;

namespace SVPM;
public static class FileHelpers
{
    private const string BaseFolderName = "Starlit\\SVPM-Starlit-Virtual-Pc-Management\\";

    public static string GetUserProfilePath()
    {
        string basePath;

        if (OperatingSystem.IsWindows())
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (OperatingSystem.IsAndroid())
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else if (OperatingSystem.IsIOS())
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        else
        {
            throw new PlatformNotSupportedException("Supported platforms are: Windows, Android, and iOS.");
        }

        return Path.Combine(basePath, BaseFolderName);
    }

    private static void EnsureDirectoryExists(string? subFolder = null)
    {
        string path = string.IsNullOrEmpty(subFolder)
            ? GetUserProfilePath()
            : Path.Combine(GetUserProfilePath(), subFolder);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    public static void CreateJsonFileIfNotExists()
    {
        if (!File.Exists(GlobalSettings.ConnectionListPath))
        {
            try
            {
                EnsureDirectoryExists();
                var emptyConnections = new List<Models.SqlConnections>();
                string emptyJson = JsonSerializer.Serialize(emptyConnections,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GlobalSettings.ConnectionListPath, emptyJson);

                Application.Current!.Windows[0].Page!.DisplayAlert("Info", "Connections file not found. A new file has been created.", "OK");
            }
            catch (Exception ex)
            {
                Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Failed to create a file with connections: {ex.Message}", "OK");
            }
        }
    }
}