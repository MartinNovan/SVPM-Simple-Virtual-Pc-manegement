using System.Text.Json;
using SVPM.Models;

namespace SVPM.Repositories;

public static class SqlConnectionRepository
{
    public static async Task<List<SqlConnection>> GetSqlConnections()
    {
        List<SqlConnection> sqlConnections = new();
        try
        {
            FileHelpers.CreateJsonFileIfNotExists();
            var json = await File.ReadAllTextAsync(GlobalSettings.ConnectionListPath);
            if (!string.IsNullOrEmpty(json))
            {
                var connections = JsonSerializer.Deserialize<List<SqlConnection>>(json) ?? new();
                connections.ForEach(sqlConnections.Add);
            }
        }
        catch (JsonException ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Invalid JSON format. A new file has been created.\nDetails: {ex.Message}", "OK");
            FileHelpers.CreateJsonFileIfNotExists();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Failed to load connection: {ex.Message}", "OK");
        }
        return sqlConnections;
    }
    
    public static async Task SaveConnectionsToFileAsync(List<SqlConnection> connections)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(connections, options);
        await File.WriteAllTextAsync(GlobalSettings.ConnectionListPath, jsonString);
    }
}