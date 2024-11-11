using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages;
using System.Text.Json;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages
{
    public partial class SqlCreateConnectionPage
    {
        private readonly string _connectionlist = GlobalSettings.ConnectionListPath;

        public SqlCreateConnectionPage(SqlConnectionPage.SqlConnections? connection = null)
        {
            InitializeComponent();
            if (connection != null)
            {
                NameEntry.Text = connection.Name;
                ServerEntry.Text = connection.ServerAddress;
                DatabaseEntry.Text = connection.DatabaseName;

                OnWindowsAuthToggled(WindowsAuthSwitch, new ToggledEventArgs(connection.UseWindowsAuth));
                WindowsAuthSwitch.IsToggled = connection.UseWindowsAuth;

                UsernameText.IsVisible = !connection.UseWindowsAuth;
                UsernameEntry.IsVisible = !connection.UseWindowsAuth;
                UsernameEntry.Text = connection.Username;

                PasswordText.IsVisible = !connection.UseWindowsAuth;
                PasswordEntry.IsVisible = !connection.UseWindowsAuth;
                PasswordEntry.Text = connection.Password;

                CertificateToggled(CertificateSwitch, new ToggledEventArgs(connection.UseCertificate));
                CertificateSwitch.IsToggled = connection.UseCertificate;
                CertificatePathEntry.Text = connection.CertificatePath;

                SaveForLater.IsChecked = true;
            }
            else
            {
                WindowsAuthSwitch.IsToggled = true;
                CertificateSwitch.IsToggled = false;
            }
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            string connectionName = NameEntry.Text;
            string server = ServerEntry.Text;
            string database = DatabaseEntry.Text;
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string certificatePath = CertificatePathEntry.Text;

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                IntegratedSecurity = WindowsAuthSwitch.IsToggled,
                TrustServerCertificate = !CertificateSwitch.IsToggled
            };

            if (!WindowsAuthSwitch.IsToggled)
            {
                builder.UserID = username;
                builder.Password = password;
            }

            if (CertificateSwitch.IsToggled && !string.IsNullOrWhiteSpace(certificatePath))
            {
                if (!await IsCertificateValidAsync(certificatePath)) return;
            }

            string connectionString = builder.ToString();

            await using SqlConnection connection = new(connectionString);
            try
            {
                await connection.OpenAsync();
                GlobalSettings.ConnectionString = connectionString;
                if (SaveForLater.IsChecked)
                {
                    await SaveConnectionAsync(connectionName, server, database, username, password, certificatePath);
                }
                await Navigation.PushAsync(new MainTabbedPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Připojení selhalo: {ex.Message}", "OK");
            }
        }


        private async Task SaveConnectionAsync(string name, string server, string database, string username, string password, string certificatePath)
        { 
            try
            {
                List<SqlConnectionPage.SqlConnections> connections;
                
                if (File.Exists(_connectionlist))
                {
                    string json = await File.ReadAllTextAsync(_connectionlist);
                    connections = JsonSerializer.Deserialize<List<SqlConnectionPage.SqlConnections>>(json) ?? new List<SqlConnectionPage.SqlConnections>();
                }
                else
                {
                    connections = new List<SqlConnectionPage.SqlConnections>();
                }
                
                var existingConnection = connections.FirstOrDefault(c => c.Name == name);
                if (existingConnection != null)
                {
                    existingConnection.ServerAddress = server;
                    existingConnection.DatabaseName = database;
                    existingConnection.UseWindowsAuth = WindowsAuthSwitch.IsToggled;
                    existingConnection.Username = username;
                    existingConnection.Password = password;
                    existingConnection.UseCertificate = CertificateSwitch.IsToggled;
                    existingConnection.CertificatePath = certificatePath;

                    bool confirm = await DisplayAlert("Info", "Existující připojení bude aktualizováno.", "OK", "Cancel");
                    if (!confirm) return;
                }
                else
                {
                    var newConnection = new SqlConnectionPage.SqlConnections
                    {
                        Name = name,
                        ServerAddress = server,
                        DatabaseName = database,
                        UseWindowsAuth = WindowsAuthSwitch.IsToggled,
                        Username = username,
                        Password = password,
                        UseCertificate = CertificateSwitch.IsToggled,
                        CertificatePath = certificatePath
                    };
                    connections.Add(newConnection);

                    await DisplayAlert("Info", "Nové připojení bylo přidáno.", "OK");
                }
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(connections, options);
                await File.WriteAllTextAsync(_connectionlist, jsonString);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nepodařilo se uložit připojení: {ex.Message}", "OK");
            }
        }

        private async Task<bool> IsCertificateValidAsync(string certPath)
        {
            try
            {
                var cert = new X509Certificate2(certPath);
                if (!cert.Verify())
                {
                    await DisplayAlert("Chyba certifikátu", "Certifikát je neplatný nebo vypršel.", "OK");
                    return false;
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Chyba", "Certifikát se nepodařilo načíst.", "OK");
                return false;
            }
            return true;
        }

        private void OnWindowsAuthToggled(object sender, ToggledEventArgs e)
        {
            UsernameText.IsVisible = PasswordText.IsVisible = UsernameEntry.IsVisible = PasswordEntry.IsVisible = !e.Value;
        }

        private void CertificateToggled(object sender, ToggledEventArgs e)
        {
            CertificatePathEntry.IsVisible = e.Value;
        }
    }
}
