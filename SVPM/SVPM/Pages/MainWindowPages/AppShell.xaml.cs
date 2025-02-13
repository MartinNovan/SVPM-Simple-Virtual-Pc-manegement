using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace SVPM.Pages.MainWindowPages;

public partial class AppShell
{
    public static ObservableCollection<Models.SqlConnections> SqlConnections { get; } = new();
    private static readonly string? Connectionlist = GlobalSettings.ConnectionListPath;
    private int _lastlySelectedItem = -1;
    public AppShell()
    {
        InitializeComponent();
    }

    private async Task LoadSqlConnectionsAsync()
    {
        try
        {
            FileHelpers.CreateJsonFileIfNotExists();
            string json = await File.ReadAllTextAsync(Connectionlist!);
            if (!string.IsNullOrEmpty(json))
            {
                var connections = JsonSerializer.Deserialize<List<Models.SqlConnections>>(json) ?? new();
                SqlConnections.Clear();
                connections.ForEach(SqlConnections.Add);
            }
        }
        catch (JsonException ex)
        {
            await DisplayAlert("Error", $"Invalid JSON format. A new file has been created.\nDetails: {ex.Message}", "OK");
            FileHelpers.CreateJsonFileIfNotExists();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load connection: {ex.Message}", "OK");
        }
    }

    private async void SqlPicker_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            if (SqlPicker.SelectedItem == null) return;
            if (SqlPicker.SelectedItem is not Models.SqlConnections connection) return;
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = connection.ServerAddress,
                InitialCatalog = connection.DatabaseName,
                IntegratedSecurity = connection.UseWindowsAuth,
                TrustServerCertificate = !connection.UseCertificate
            };

            if (!connection.UseWindowsAuth)
            {
                builder.UserID = connection.Username;
                builder.Password = connection.Password;
            }

            if (connection.UseCertificate && !string.IsNullOrWhiteSpace(connection.CertificatePath))
            {
                builder.ServerCertificate = connection.CertificatePath;
            }

            await using var sqlConnection = new SqlConnection(builder.ToString());
            await sqlConnection.OpenAsync();
            GlobalSettings.ConnectionString = builder.ToString();
            _lastlySelectedItem = SqlPicker.SelectedIndex;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
        }
    }

    private async void SqlPicker_OnLoaded(object? sender, EventArgs e)
    {
        await LoadSqlConnectionsAsync();
        SqlPicker.ItemsSource = SqlConnections;
        if(_lastlySelectedItem != -1 && _lastlySelectedItem != null)  SqlPicker.SelectedIndex = _lastlySelectedItem;
    }

    private async void PullFromDatabase(object? sender, EventArgs e)
    {
        if(GlobalSettings.ConnectionString == null) return;
        await Navigation.PushAsync(new LoadingPage(false));
    }

    private async void PushToDatabase(object? sender, EventArgs e)
    {
        if(GlobalSettings.ConnectionString == null) return;
        await Navigation.PushAsync(new LoadingPage(true));

    }
}