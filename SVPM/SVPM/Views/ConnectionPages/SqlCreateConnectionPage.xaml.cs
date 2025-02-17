using System.Text.Json;
using Microsoft.Data.SqlClient;
using SVPM.Views.MainWindowPages;
using SqlConnection = SVPM.Models.SqlConnection;

namespace SVPM.Views.ConnectionPages
{
    public partial class SqlCreateConnectionPage
    {
        private readonly string _connectionlist = GlobalSettings.ConnectionListPath;

        public SqlCreateConnectionPage(SqlConnection? connection = null)
        {
            InitializeComponent();
            if (connection != null)
            {
                PopulateFields(connection);
            }
            else
            {
                WindowsAuthSwitch.IsToggled = true;
                CertificateSwitch.IsToggled = false;
            }
        }

        private void PopulateFields(SqlConnection? connection)
        {
            if (connection != null)
            {
                NameEntry.Text = connection.Name ?? string.Empty;
                ServerEntry.Text = connection.ServerAddress ?? string.Empty;
                DatabaseEntry.Text = connection.DatabaseName ?? string.Empty;

                WindowsAuthSwitch.IsToggled = connection.UseWindowsAuth;
                SetVisibility(!connection.UseWindowsAuth);
                UsernameEntry.Text = connection.Username ?? string.Empty;
                PasswordEntry.Text = connection.Password ?? string.Empty;

                CertificateSwitch.IsToggled = connection.UseCertificate;
            }

            SaveForLater.IsChecked = true;
        }

        private void SetVisibility(bool isVisible)
        {
            UsernameText.IsVisible = isVisible;
            UsernameEntry.IsVisible = isVisible;
            PasswordText.IsVisible = isVisible;
            PasswordEntry.IsVisible = isVisible;
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                await ShowErrorMessage("Please fill in all required fields.");
                return;
            }

            var connectionString = BuildConnectionString();

            await using Microsoft.Data.SqlClient.SqlConnection connection = new(connectionString);
            try
            {
                IsProcessing.IsVisible = true;
                await connection.OpenAsync();
                GlobalSettings.ConnectionString = connectionString;

                if (SaveForLater.IsChecked)
                {
                    await SaveConnectionAsync();
                }
                IsProcessing.IsVisible = false;
                await Navigation.PushAsync(new LoadingPage(false));
            }
            catch (SqlException sqlEx)
            {
                IsProcessing.IsVisible = false;
                await ShowErrorMessage($"Database connection error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                IsProcessing.IsVisible = false;
                await ShowErrorMessage($"Connection failed: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            return !string.IsNullOrWhiteSpace(NameEntry.Text) &&
                   !string.IsNullOrWhiteSpace(ServerEntry.Text) &&
                   !string.IsNullOrWhiteSpace(DatabaseEntry.Text);
        }

        private string BuildConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = ServerEntry.Text,
                InitialCatalog = DatabaseEntry.Text,
                IntegratedSecurity = WindowsAuthSwitch.IsToggled,
                TrustServerCertificate = !CertificateSwitch.IsToggled
            };

            if (!WindowsAuthSwitch.IsToggled)
            {
                builder.UserID = UsernameEntry.Text;
                builder.Password = PasswordEntry.Text;
            }

            return builder.ToString();
        }

        private async Task SaveConnectionAsync()
        {
            try
            {
                List<SqlConnection> connections = await LoadConnectionsAsync();
                var existingConnection = connections.FirstOrDefault(c => c.Name == NameEntry.Text);

                if (existingConnection != null)
                {
                    UpdateExistingConnection(existingConnection);
                    bool confirm = await DisplayAlert("Info", "Existing connection will be updated.", "OK", "Cancel");
                    if (!confirm) return;
                }
                else
                {
                    connections.Add(CreateNewConnection());
                    await DisplayAlert("Info", "New connection has been added.", "OK");
                }

                await SaveConnectionsToFileAsync(connections);
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Failed to save connection: {ex.Message}");
            }
        }

        private void UpdateExistingConnection(SqlConnection existingConnection)
        {
            existingConnection.ServerAddress = ServerEntry.Text;
            existingConnection.DatabaseName = DatabaseEntry.Text;
            existingConnection.UseWindowsAuth = WindowsAuthSwitch.IsToggled;
            existingConnection.Username = UsernameEntry.Text;
            existingConnection.Password = PasswordEntry.Text;
            existingConnection.UseCertificate = CertificateSwitch.IsToggled;
            existingConnection.CertificatePath = CertificatePathEntry.Text;
        }

        private SqlConnection CreateNewConnection()
        {
            return new SqlConnection
            {
                Name = NameEntry.Text,
                ServerAddress = ServerEntry.Text,
                DatabaseName = DatabaseEntry.Text,
                UseWindowsAuth = WindowsAuthSwitch.IsToggled,
                Username = UsernameEntry.Text,
                Password = PasswordEntry.Text,
                UseCertificate = CertificateSwitch.IsToggled,
                CertificatePath = CertificatePathEntry.Text
            };
        }

        private async Task<List<SqlConnection>> LoadConnectionsAsync()
        {
            if (File.Exists(_connectionlist))
            {
                string json = await File.ReadAllTextAsync(_connectionlist);
                return JsonSerializer.Deserialize<List<SqlConnection>>(json) ?? new List<SqlConnection>();
            }
            return new List<SqlConnection>();
        }

        private async Task SaveConnectionsToFileAsync(List<SqlConnection> connections)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(connections, options);
            await File.WriteAllTextAsync(_connectionlist, jsonString);
        }

        private async Task ShowErrorMessage(string message)
        {
            await DisplayAlert("Error", message, "OK");
        }

        private void OnWindowsAuthToggled(object sender, ToggledEventArgs e)
        {
            SetVisibility(!e.Value);
        }

        private void CertificateToggled(object sender, ToggledEventArgs e)
        {
            CertificatePathEntry.IsVisible = e.Value;
            CertificateText.IsVisible = e.Value;
        }
    }
}