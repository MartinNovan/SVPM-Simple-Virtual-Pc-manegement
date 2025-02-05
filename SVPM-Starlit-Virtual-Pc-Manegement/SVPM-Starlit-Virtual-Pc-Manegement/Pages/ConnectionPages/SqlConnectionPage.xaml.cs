using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages
{
    public partial class SqlConnectionPage
    {
        public ObservableCollection<Models.SqlConnections> SqlConnections { get; } = new();
        private readonly string? _connectionlist = GlobalSettings.ConnectionListPath;

        public SqlConnectionPage()
        {
            InitializeComponent();
        }

        private async void SqlConnectionsListOnLoaded(object? sender, EventArgs e)
        {
            await LoadSqlConnectionsAsync();
            SqlConnectionListView.ItemsSource = SqlConnections;
        }

        private async Task LoadSqlConnectionsAsync()
        {
            try
            {
                FileHelpers.CreateJsonFileIfNotExists();
                string json = await File.ReadAllTextAsync(_connectionlist!);
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

        private async void SqlConnection_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is not Models.SqlConnections connection) return;
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

                IsProcessing.IsVisible = true;

                await using var sqlConnection = new SqlConnection(builder.ToString());
                await sqlConnection.OpenAsync();

                GlobalSettings.ConnectionString = builder.ToString();
                IsProcessing.IsVisible = false;
                await Navigation.PushAsync(new LoadingPage(false));
            }
            catch (Exception ex)
            {
                IsProcessing.IsVisible = false;
                await DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
            }
        }

        private async void CreateConnection_Clicked(object? sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new SqlCreateConnectionPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void RefreshButton_Clicked(object? sender, EventArgs e)
        {
            try
            {
                await LoadSqlConnectionsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message , "OK");
            }
        }

        private async void EditConnection_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not Models.SqlConnections connection) return;
                await Navigation.PushAsync(new SqlCreateConnectionPage(connection));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not Models.SqlConnections connection) return;

                bool confirm = await DisplayAlert("Warning!", "Do you really want to delete a saved connection?", "OK", "Cancel");
                if (!confirm) return;
                
                if (File.Exists(_connectionlist))
                {
                    string json = await File.ReadAllTextAsync(_connectionlist);
                    var connections = JsonSerializer.Deserialize<List<Models.SqlConnections>>(json) ?? new();
                    if (connections.RemoveAll(c => c.Name == connection.Name) > 0)
                    {
                        string updatedJson = JsonSerializer.Serialize(connections, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(_connectionlist, updatedJson);
                        SqlConnections.Remove(connection);
                        await DisplayAlert("Success", "The connection has been successfully deleted.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Connection not found.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "The connections file was not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete connection: {ex.Message}", "OK");
            }
        }
    }
}
