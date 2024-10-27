using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages
{
    public partial class AccountsPage
    {
        private List<Account>? _accounts;

        public AccountsPage()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private async void LoadAccounts()
        {
            try
            {
                string? connectionString = GlobalSettings.ConnectionString;
                _accounts = new List<Account>();

                await using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    await using (SqlCommand command = new SqlCommand($@"SELECT a.AccountID, a.VirtualPcID, a.Username, a.Password, a.IsAdmin, a.LastUpdated, s.VirtualPcName, c.FullName FROM {GlobalSettings.AccountTable} a INNER JOIN {GlobalSettings.VirtualPcTable} s ON a.VirtualPcID = s.VirtualPcID INNER JOIN {GlobalSettings.CustomerTable} c ON s.CustomerID = c.CustomerID", connection))
                    {
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                _accounts.Add(new Account
                                {
                                    AccountId = reader.GetGuid(0),
                                    VirtualPcId = reader.GetGuid(1),
                                    Username = reader.IsDBNull(2) ? " " : reader.GetString(2),
                                    Password = reader.IsDBNull(3) ? " " : reader.GetString(3),
                                    IsAdmin = reader.GetBoolean(4),
                                    LastUpdated = reader.GetDateTime(5).ToString("g"),
                                    ServerName = reader.IsDBNull(6) ? " " : reader.GetString(6),
                                    FullName = reader.IsDBNull(7) ? " " : reader.GetString(7)
                                });
                            }
                        }
                    }
                }

                AccountsListView.ItemsSource = _accounts;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                AccountsListView.ItemsSource = _accounts;
            }
            else
            {
                if (_accounts != null)
                    AccountsListView.ItemsSource = _accounts
                        .Where(a => a is { Username: not null, ServerName: not null, FullName: not null } &&
                                    (a.Username.ToLower().Contains(searchText) ||
                                     a.ServerName.ToLower().Contains(searchText) ||
                                     a.FullName.ToLower().Contains(searchText)))
                        .ToList();
            }
        }

        public class Account
        {
            public Guid AccountId { get; set; }
            public Guid VirtualPcId {get; set; }
            public string? Username { get; init; }
            public string? Password { get; set; }
            public bool IsAdmin { get; set; }
            public string? LastUpdated { get; set; }
            public string? ServerName { get; init; }
            public string? FullName { get; init; } 
        }
    }
}
