using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages;

public partial class SqlConnectionPage : ContentPage
{
    private ObservableCollection<SqlConnections> _sqlConnections;
    private readonly string _connectionlist = GlobalSettings.ConnectionListPath;

    public SqlConnectionPage()
    {
        InitializeComponent();

        // Inicializace kolekce s prázdným konstruktor
        _sqlConnections = new ObservableCollection<SqlConnections>();

        BindingContext = this;

        // Načteme uložená připojení
        LoadSqlConnections();
    }

    private async void LoadSqlConnections()
    {
        try
        {
            if (File.Exists(_connectionlist))
            {
                string json = File.ReadAllText(_connectionlist);
                var connections = JsonSerializer.Deserialize<List<SqlConnections>>(json) ?? new List<SqlConnections>();
            
                // Vyčistíme kolekci
                _sqlConnections.Clear();
                var createConnection = new SqlConnections { Name = "Přidat připojení", IsCreateNewOption = true };
                _sqlConnections.Add(createConnection);
                // Přidáme uložená připojení
                foreach (var connection in connections)
                {
                    _sqlConnections.Add(connection);
                }
            }
            else
            {
                var emptyConnections = new List<SqlConnections>();
                string emptyJson = JsonSerializer.Serialize(emptyConnections);
                File.WriteAllText(_connectionlist, emptyJson);

                await DisplayAlert("Info", "Connections file not found. A new file has been created.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load connections: {ex.Message}", "OK");
        }
    }

    private async void SqlConnection_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is SqlConnections connection)
            if (connection.IsCreateNewOption)
            {
                await Navigation.PushAsync(new SqlCreateConnectionPage());
            }
            else
            {
                await DisplayAlert("Connection Selected", $"Selected: {connection.Name}", "OK");
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
        public bool IsCreateNewOption { get; set; }
    }
}
