using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class SqlConnectionPage : ContentPage
    {
        public SqlConnectionPage()
        {
            InitializeComponent();
            WindowsAuthSwitch.IsToggled = true; // Nastavení přepínače na zapnutý
            OnWindowsAuthToggled(WindowsAuthSwitch, new ToggledEventArgs(true)); // Nastavení viditelnosti polí
            TrustCertificateSwitch.IsToggled = true; // Nastavení přepínače na zapnutý
            CertificateToggled(TrustCertificateSwitch, new ToggledEventArgs(true)); // Nastavení viditelnosti polí
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            // Získání údajů z textových polí
            string server = ServerEntry.Text;
            string database = DatabaseEntry.Text;
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string certificatePath = CertificatePathEntry.Text;

            // Kontrola, zda jsou vyplněny server a databáze
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

            // Vytvoření dynamického connection stringu
            string connectionString = $"Server={server};Database={database};";

            if (WindowsAuthSwitch.IsToggled)
            {
                // Windows Authentication
                connectionString += "Integrated Security=True;";
            }
            else
            {
                // SQL Server Authentication
                connectionString += $"User Id={username};Password={password};";
            }

            if (!TrustCertificateSwitch.IsToggled && !string.IsNullOrWhiteSpace(certificatePath))
            {
                if (!IsCertificateValid(certificatePath))
                {
                    await DisplayAlert("Chyba", "Certifikát není platný.", "OK");
                    return;
                }
                connectionString += $"Certificate={certificatePath};"; // Přidání certifikátu do connection stringu
            }
            else if (TrustCertificateSwitch.IsToggled)
            {
                connectionString += "TrustServerCertificate=True;"; // Důvěřovat certifikátu, pokud je povoleno
            }

            using SqlConnection connection = new(connectionString);
            try
            {
                // Pokus o otevření připojení
                await connection.OpenAsync();
                GlobalSettings.ConnectionString = connectionString;
                await Navigation.PushAsync(new MainTabbedPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Připojení selhalo: {ex.Message}", "OK");
            }
        }

        private async void OnDiscoverDatabasesButtonClicked(object sender, EventArgs e)
        {
            string server = ServerEntry.Text;

            if (string.IsNullOrWhiteSpace(server))
            {
                await DisplayAlert("Chyba", "Zadejte prosím adresu serveru.", "OK");
                return;
            }

            string connectionString = $"Server={server};Integrated Security=True;"; // Pokud používáte Windows Auth

            try
            {
                using SqlConnection connection = new(connectionString);
                await connection.OpenAsync();

                // Dotaz pro načtení databází
                string query = "SELECT name FROM sys.databases;";
                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                List<string> databases = new List<string>();
                while (await reader.ReadAsync())
                {
                    databases.Add(reader.GetString(0));
                }

                // Zde můžete zobrazit seznam databází například v ListView
                await DisplayAlert("Databáze", string.Join(", ", databases), "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Načtení databází selhalo: {ex.Message}", "OK");
            }
        }

        private void OnWindowsAuthToggled(object sender, ToggledEventArgs e)
        {
            // Změna stavu povolení pro uživatelské jméno a heslo
            UsernameEntry.IsEnabled = !e.Value;
            PasswordEntry.IsEnabled = !e.Value;
        }

        private void CertificateToggled(object sender, ToggledEventArgs e)
        {
            // Změna stavu povolení pro cestu k certifikátu
            CertificatePathEntry.IsEnabled = !e.Value;
        }

        private bool IsCertificateValid(string certificatePath)
        {
            try
            {
                // Načtení certifikátu ze zadané cesty
                X509Certificate2 certificate = new X509Certificate2(certificatePath);

                // Ověření, zda certifikát není propadlý
                if (certificate.NotAfter < DateTime.Now)
                {
                    DisplayAlert("Chyba", "Certifikát je propadlý.", "OK");
                    return false;
                }

                // Ověření, zda certifikát byl vydán důvěryhodnou certifikační autoritou
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
