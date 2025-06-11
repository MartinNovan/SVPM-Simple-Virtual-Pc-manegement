using System.Text.Json;

namespace SVPM;

public static class UpdateChecker
{
    private static async Task<string?> GetLatestReleaseVersion()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName/1.0");

            var url = "https://api.github.com/repos/MartinNovan/Test/releases/latest";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var tag = doc.RootElement.GetProperty("tag_name").GetString();
            return tag?.TrimStart('v');
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Failed to check for updates: {ex.Message}", "OK");
        }
        return null;
    }

    public static async Task<bool> IsNewVersionAvailable()
    {
        if (string.IsNullOrEmpty(AppInfo.VersionString)) return false;
        try
        {
            var latest = await GetLatestReleaseVersion();
            if (latest == null) return false;

            var current = Version.Parse(AppInfo.VersionString);
            var latestVersion = Version.Parse(latest);
            if (current != Version.Parse(GlobalSettings.Version))
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Warning", "Version is not the same as the version of this app.", "OK");
                return latestVersion > Version.Parse(GlobalSettings.Version);
            }
            return latestVersion > current;
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Failed to check for updates: {ex.Message}", "OK");
        }
        return false;
    }
}