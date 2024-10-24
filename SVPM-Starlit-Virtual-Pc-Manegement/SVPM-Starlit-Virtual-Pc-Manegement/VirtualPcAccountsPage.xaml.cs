using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class VirtualPcAccountsPage : ContentPage
    {
        private Guid _VirtualPcID;

        public VirtualPcAccountsPage(Guid VirtualPcID)
        {
            InitializeComponent();
            _VirtualPcID = VirtualPcID;
            LoadVirtualPc();
            LoadAccounts();
        }

        private async void LoadVirtualPc()
        {
            try
            {
                string connectionString = GlobalSettings.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand($"SELECT CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName FROM {GlobalSettings.VirtualPcTable} WHERE VirtualPcID = @VirtualPcID", connection))
                    {
                        command.Parameters.AddWithValue("@VirtualPcID", _VirtualPcID);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var virtualpc = new VirtualPc
                                {
                                    CPU_Cores = reader.GetInt32(0), // Opravený index
                                    RAM_Size_GB = reader.GetInt32(1), // Opravený index
                                    Disk_Size_GB = reader.GetInt32(2), // Opravený index
                                    Virtual_Pc_Name = reader.IsDBNull(3) ? "(Bez Jména)" : reader.GetString(3) // Opravený index
                                };
                                BindingContext = virtualpc;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }
        }

        private async void LoadAccounts()
        {
            try
            {
                string connectionString = GlobalSettings.ConnectionString;
                List<Account> accounts = new List<Account>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand($"SELECT AccountID, Username, Password, IsAdmin, LastUpdated FROM {GlobalSettings.AccountTable} WHERE VirtualPcID = @VirtualPcID", connection))
                    {
                        command.Parameters.AddWithValue("@VirtualPcID", _VirtualPcID);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                accounts.Add(new Account
                                {
                                    AccountID = reader.GetGuid(0),
                                    Username = reader.GetString(1),
                                    Password = reader.GetString(2),
                                    IsAdmin = reader.GetBoolean(3), // Opravený datový typ
                                    LastUpdated = reader.GetDateTime(4).ToString("g") // Opravený datový typ a formát
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

        public class VirtualPc
        {
            public Guid VirtualPcID { get; set; }
            public int CPU_Cores { get; set; }
            public int RAM_Size_GB { get; set; }
            public int Disk_Size_GB { get; set; }
            public string Virtual_Pc_Name { get; set; }
        }

        public class Account
        {
            public Guid AccountID { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool IsAdmin { get; set; }
            public string LastUpdated { get; set; } // Zůstává jako string pro zobrazení
        }
    }
}
