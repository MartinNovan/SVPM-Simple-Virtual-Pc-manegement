using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class AccountsPage : ContentPage
    {
        private List<Account> accounts;

        public AccountsPage()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private async void LoadAccounts()
        {
            try
            {
                string connectionString = GlobalSettings.ConnectionString;
                accounts = new List<Account>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand($@"SELECT a.AccountID, a.ServerID, a.Username, a.Password, a.IsAdmin, a.LastUpdated, s.VirtualPcName, c.FullName FROM {GlobalSettings.AccountTable} a INNER JOIN {GlobalSettings.VirtualPcTable} s ON a.ServerID = s.ServerID INNER JOIN {GlobalSettings.CustomerTable} c ON s.CustomerID = c.CustomerID", connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                accounts.Add(new Account
                                {
                                    AccountID = reader.GetInt32(0),
                                    ServerID = reader.GetInt32(1),
                                    Username = reader.IsDBNull(2) ? " " : reader.GetString(2),
                                    Password = reader.IsDBNull(3) ? " " : reader.GetString(3),
                                    IsAdmin = reader.GetBoolean(4),
                                    LastUpdated = reader.GetDateTime(5).ToString("g"),
                                    ServerName = reader.IsDBNull(6) ? " " : reader.GetString(6), // Získání jména serveru
                                    FullName = reader.IsDBNull(7) ? " " : reader.GetString(7) // Získání jména vlastníka
                                });
                            }
                        }
                    }
                }

                AccountsListView.ItemsSource = accounts;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                AccountsListView.ItemsSource = accounts; // Pokud je prázdné, zobrazí všechny
            }
            else
            {
                AccountsListView.ItemsSource = accounts
                    .Where(a => a.Username.ToLower().Contains(searchText) ||
                                 a.ServerName.ToLower().Contains(searchText) ||
                                 a.FullName.ToLower().Contains(searchText)) // Přidáno filtrování podle názvu serveru a vlastníka
                    .ToList();
            }
        }

        public class Account
        {
            public int AccountID { get; set; }
            public int ServerID { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public bool IsAdmin { get; set; }
            public string? LastUpdated { get; set; } // Zůstává jako string pro zobrazení
            public string? ServerName { get; set; } // Jméno serveru
            public string? FullName { get; set; } // Vlastník serveru
        }
    }
}
