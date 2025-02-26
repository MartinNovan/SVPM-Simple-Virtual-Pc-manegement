using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SVPM.Views.MainPages;
using SqlConnection = SVPM.Models.SqlConnection;

namespace SVPM;
//TODO Dodělat setting page, ukládat neuložené hodnoty například to TEMP souboru, možnost automaticky (např. 5min intervali) ukládat do databáze a kontrolovat změny v databázi a dodělat setting JSON soubor
public partial class AppShell
{
    public static ObservableCollection<SqlConnection> SqlConnections { get; } = new();
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
                var connections = JsonSerializer.Deserialize<List<SqlConnection>>(json) ?? new();
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
            if (SqlPicker.SelectedItem is not SqlConnection connection) return;
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

            await using var sqlConnection = new Microsoft.Data.SqlClient.SqlConnection(builder.ToString());
            await sqlConnection.OpenAsync();
            GlobalSettings.ConnectionString = builder.ToString();
            _lastlySelectedItem = SqlPicker.SelectedIndex;
        }
        catch (Exception ex)
        {
            SqlPicker.SelectedIndex = -1;
            await DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
        }
    }

    private async void SqlPicker_OnLoaded(object? sender, EventArgs e)
    {
        await LoadSqlConnectionsAsync();
        SqlPicker.ItemsSource = SqlConnections;
        if(_lastlySelectedItem != -1)  SqlPicker.SelectedIndex = _lastlySelectedItem;
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

    private async void CheckBox_OnCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (UploadAllCheckBox.IsChecked)
        {
            await DisplayAlert("Warning", "With this setting 'ON' all your local data will be pushed to database regardless if they were or weren't changed!", "OK" );
            Console.WriteLine("Turning on this mode.");
        }
        else
        {
            Console.WriteLine("Turning it off");
        }
    }
}