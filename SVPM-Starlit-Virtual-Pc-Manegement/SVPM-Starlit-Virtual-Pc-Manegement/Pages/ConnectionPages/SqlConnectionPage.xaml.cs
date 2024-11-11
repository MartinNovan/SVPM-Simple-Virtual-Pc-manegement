using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages
{
    public partial class SqlConnectionPage
    {
        private readonly ObservableCollection<SqlConnections> _sqlConnections = new();
        private readonly string _connectionlist = GlobalSettings.ConnectionListPath;

        public SqlConnectionPage()
        {
            InitializeComponent();
            BindingContext = this;
            GlobalSettings.EnsureDirectoryExists();
            CreateJsonIfNeeded();
            LoadSqlConnections();
        }

        private void CreateJsonIfNeeded()
        {
            if (!File.Exists(_connectionlist))
            {
                try
                {
                    GlobalSettings.EnsureDirectoryExists();
                    var emptyConnections = new List<SqlConnections>();
                    string emptyJson = JsonSerializer.Serialize(emptyConnections, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_connectionlist, emptyJson);
                    DisplayAlert("Info", "Soubor s připojeními nebyl nalezen. Byl vytvořen nový soubor.", "OK");
                }
                catch (Exception ex)
                {
                    DisplayAlert("Chyba", $"Nepodařilo se vytvořit soubor s připojeními: {ex.Message}", "OK");
                }
            }
        }

        private async void LoadSqlConnections()
        {
            try
            {
                if (!File.Exists(_connectionlist))
                {
                    CreateJsonIfNeeded();
                }

                string json = await File.ReadAllTextAsync(_connectionlist);

                if (string.IsNullOrWhiteSpace(json))
                {
                    await DisplayAlert("Chyba", "Soubor s připojeními je prázdný. Byl vytvořen nový soubor.", "OK");
                    CreateJsonIfNeeded();
                    return;
                }

                var connections = JsonSerializer.Deserialize<List<SqlConnections>>(json);
                if (connections == null)
                {
                    CreateJsonIfNeeded();
                    await DisplayAlert("Chyba", "Soubor s připojeními má nesprávný formát. Očekává se pole připojení.", "OK");
                    return;
                }

                _sqlConnections.Clear();
                foreach (var connection in connections)
                {
                    _sqlConnections.Add(connection);
                }
            }
            catch (JsonException)
            {
                await DisplayAlert("Chyba", "Soubor JSON je poškozený nebo má nesprávný formát. Byl vytvořen nový soubor.", "OK");
                CreateJsonIfNeeded();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst připojení: {ex.Message}", "OK");
            }
        }

        private async void SqlConnection_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is SqlConnections connection)
            {
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

                string connectionString = builder.ToString();

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

        private async void CreateConnection_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new SqlCreateConnectionPage());
        }

        private void RefreshButton_Clicked(object? sender, EventArgs e)
        {
            _sqlConnections.Clear();
            LoadSqlConnections();
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

            if (connection != null)
            {
                bool confirm = await DisplayAlert("Warning!", "Vážně chcete odstranit uložené připojení.", "OK", "Cancel");
                if (!confirm) return;

                await DeleteConnectionAsync(connection);
            }
        }

        private async Task DeleteConnectionAsync(SqlConnections connection)
        {
            try
            {
                if (File.Exists(_connectionlist))
                {
                    var connections = await LoadConnectionsFromFileAsync();
                    var connectionToRemove = connections.FirstOrDefault(c => c.Name == connection.Name);
                    if (connectionToRemove != null)
                    {
                        connections.Remove(connectionToRemove);
                        string updatedJson = JsonSerializer.Serialize(connections, new JsonSerializerOptions { WriteIndented = true });
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

        public ObservableCollection<SqlConnections> Connections => _sqlConnections;

        private async Task<List<SqlConnections>> LoadConnectionsFromFileAsync()
        {
            string json = await File.ReadAllTextAsync(_connectionlist);
            return JsonSerializer.Deserialize<List<SqlConnections>>(json) ?? new List<SqlConnections>();
        }

        public class SqlConnections
        {
            public string? Name { get; init; }
            public string? ServerAddress { get; set; }
            public string? DatabaseName { get; set; }
            public bool UseWindowsAuth { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public bool UseCertificate { get; set; }
            public string? CertificatePath { get; set; }
        }
    }
}
