using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages
{
    public partial class SqlCreateConnectionPage
    {
        public SqlCreateConnectionPage(SqlConnectionPage.SqlConnections connection = null)
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
                CertificateText.IsVisible = connection.UseCertificate;
                CertificatePathEntry.IsVisible = connection.UseCertificate;
                CertificatePathEntry.Text = connection.CertificatePath;
                
                Save.IsChecked = true;
            }
            else
            {
                WindowsAuthSwitch.IsToggled = true;
                OnWindowsAuthToggled(WindowsAuthSwitch, new ToggledEventArgs(true));
                CertificateSwitch.IsToggled = true;
                CertificateToggled(CertificateSwitch, new ToggledEventArgs(true));   
            }
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            string server = ServerEntry.Text;
            string database = DatabaseEntry.Text;
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string certificatePath = CertificatePathEntry.Text;
            
            if (string.IsNullOrWhiteSpace(server))
            {
                await DisplayAlert("Chyba", "Zadejte prosím adresu serveru.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(database))
            {
                await DisplayAlert("Chyba", "Zadejte prosím název databáze.", "OK");
                return;
            }
            
            string connectionString = $"Server={server};Database={database};";

            if (WindowsAuthSwitch.IsToggled)
            {
                connectionString += "Integrated Security=True;";
            }
            else
            {
                connectionString += $"User Id={username};Password={password};";
            }

            if (CertificateSwitch.IsToggled && !string.IsNullOrWhiteSpace(certificatePath))
            {
                if (!IsCertificateValid(certificatePath))
                {
                    await DisplayAlert("Chyba", "Certifikát není platný.", "OK");
                    return;
                }
                connectionString += $"Certificate={certificatePath};";
            }
            else if (!CertificateSwitch.IsToggled)
            {
                connectionString += "TrustServerCertificate=True;";
            }

            await using SqlConnection connection = new(connectionString);
            try
            {
                await connection.OpenAsync();
                GlobalSettings.ConnectionString = connectionString;
                await Navigation.PushAsync(new MainTabbedPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Připojení selhalo: {ex.Message}", "OK");
            }
        }

        private void OnWindowsAuthToggled(object sender, ToggledEventArgs e)
        {
            UsernameEntry.IsVisible = !e.Value;
            UsernameText.IsVisible = !e.Value;
            PasswordEntry.IsVisible = !e.Value;
            PasswordText.IsVisible = !e.Value;
        }

        private void CertificateToggled(object sender, ToggledEventArgs e)
        {
            CertificatePathEntry.IsVisible = e.Value;
            CertificateText.IsVisible = e.Value;
        }

        private bool IsCertificateValid(string certificatePath)
        {
            try
            {
                X509Certificate2 certificate = new X509Certificate2(certificatePath);
                
                if (certificate.NotAfter < DateTime.Now)
                {
                    DisplayAlert("Chyba", "Certifikát je propadlý.", "OK");
                    return false;
                }
                
                X509Chain chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;

                bool isValid = chain.Build(certificate);
                if (!isValid)
                {
                    DisplayAlert("Chyba", "Certifikát nebyl vydán důvěryhodnou certifikační autoritou.", "OK");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                DisplayAlert("Chyba", $"Nelze načíst certifikát: {ex.Message}", "OK");
                return false;
            }
        }
    }
}
