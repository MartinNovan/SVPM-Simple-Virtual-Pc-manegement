using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages;

public partial class SqlConnectionPage
{
    private readonly ObservableCollection<SqlConnections> _sqlConnections = new();
    private readonly string _connectionlist = GlobalSettings.ConnectionListPath;

    public SqlConnectionPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadSqlConnections();
    }
    private async void LoadSqlConnections()
    {
        try
        {
            if (File.Exists(_connectionlist))
            {
                string json = await File.ReadAllTextAsync(_connectionlist);
                
                try
                {
                    var jsonDoc = JsonDocument.Parse(json);
                    if (!jsonDoc.RootElement.ValueKind.Equals(JsonValueKind.Array))
                    {
                        await DisplayAlert("Chyba", "Soubor s připojeními má nesprávný formát. Očekává se pole připojení.", "OK");
                        return;
                    }
                }
                catch (JsonException)
                {
                    await DisplayAlert("Chyba", "Soubor JSON je poškozený nebo má nesprávný formát.", "OK");
                    return;
                }
                
                var connections = JsonSerializer.Deserialize<List<SqlConnections>>(json) ?? new List<SqlConnections>();
                foreach (var connection in connections)
                {
                    _sqlConnections.Add(connection);
                }
            }
            else
            {
                await CreateEmptyConnectionsFile();
                await DisplayAlert("Info", "Soubor s připojeními nebyl nalezen. Byl vytvořen nový soubor.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba", $"Nepodařilo se načíst připojení: {ex.Message}", "OK");
        }
    }


    private async Task CreateEmptyConnectionsFile()
    {
        var emptyConnections = new List<SqlConnections>();
        string emptyJson = JsonSerializer.Serialize(emptyConnections);
        await File.WriteAllTextAsync(_connectionlist, emptyJson);
    }

    private async void SqlConnection_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is SqlConnections connection)
        {
            string connectionString = $"Server={connection.ServerAddress};Database={connection.DatabaseName};";

            if (connection.UseWindowsAuth)
            {
                connectionString += "Integrated Security=True;";
            }
            else
            {
                connectionString += $"User Id={connection.Username};Password={connection.Password};";
            }

            if (connection.UseCertificate)
            {
                connectionString += $"Certificate={connection.CertificatePath};";
            }
            else
            {
                connectionString += "TrustServerCertificate=True;";
            }

            await using SqlConnection sqlConnection = new(connectionString);
            try
            {
                await sqlConnection.OpenAsync();
                GlobalSettings.ConnectionString = connectionString;
                await Navigation.PushAsync(new MainTabbedPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Připojení selhalo: {ex.Message}", "OK");
            }
        }
    }

    public ObservableCollection<SqlConnections> Connections => _sqlConnections;

    public class SqlConnections
    {
        public string? Name { get; set; }
        public string? ServerAddress { get; set; }
        public string? DatabaseName { get; set; }
        public bool UseWindowsAuth { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool UseCertificate { get; set; }
        public string? CertificatePath { get; set; }
    }

    private async void EditConnection_Clicked(object sender, EventArgs e)
    {
        try
        {
            var button = (ImageButton)sender;
            var connection = (SqlConnections)button.BindingContext;

            if (connection != null)
            {
                await Navigation.PushAsync(new SqlCreateConnectionPage(connection));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba", $"Připojení selhalo: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;
        var connection = (SqlConnections)button.BindingContext;
        string? connectionName = connection.Name;

        bool confirm = await DisplayAlert("Warning!", "Vážně chcete odstranit uložené připojení.", "OK", "Cancel");
        if (!confirm) return;

        try
        {
            if (File.Exists(_connectionlist))
            {
                string json = await File.ReadAllTextAsync(_connectionlist);
                var connections = JsonSerializer.Deserialize<List<SqlConnections>>(json) ?? new List<SqlConnections>();

                var connectionToRemove = connections.FirstOrDefault(c => c.Name == connectionName);
                if (connectionToRemove != null)
                {
                    connections.Remove(connectionToRemove);

                    string updatedJson = JsonSerializer.Serialize(connections);
                    await File.WriteAllTextAsync(_connectionlist, updatedJson);

                    await DisplayAlert("Success", "Připojení bylo úspěšně smazané.", "OK");

                    _sqlConnections.Clear();
                    LoadSqlConnections();
                }
                else
                {
                    await DisplayAlert("Error", "Připojení nenalezeno.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Soubor s připojeními se nenašel.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Nepodařilo se vymazat připojení: {ex.Message}", "OK");
        }
    }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SqlCreateConnectionPage());
    }

    private void RefreshButton_Clicked(object? sender, EventArgs e)
    {
        _sqlConnections.Clear();
        LoadSqlConnections();
    }
}
