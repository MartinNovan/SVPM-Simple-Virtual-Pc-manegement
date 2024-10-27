using Microsoft.Data.SqlClient;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.SubWindowPages
{
    public partial class VirtualPcAccountsPage
    {
        private readonly Guid _virtualPcId;

        public VirtualPcAccountsPage(Guid virtualPcId)
        {
            InitializeComponent();
            _virtualPcId = virtualPcId;
            LoadVirtualPc();
            LoadAccounts();
        }

        private async void LoadVirtualPc()
        {
            try
            {
                string? connectionString = GlobalSettings.ConnectionString;

                await using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                await using SqlCommand command = new SqlCommand($"SELECT CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName FROM {GlobalSettings.VirtualPcTable} WHERE VirtualPcID = @VirtualPcID", connection);
                command.Parameters.AddWithValue("@VirtualPcID", _virtualPcId);

                await using SqlDataReader reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return;
                var virtualpc = new VirtualPc
                {
                    CpuCores = reader.GetInt32(0), 
                    RamSizeGb = reader.GetInt32(1), 
                    DiskSizeGb = reader.GetInt32(2), 
                    VirtualPcName = reader.IsDBNull(3) ? "(Bez Jména)" : reader.GetString(3) 
                };
                BindingContext = virtualpc;
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
                string? connectionString = GlobalSettings.ConnectionString;
                List<Account> accounts = new List<Account>();

                await using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    await using (SqlCommand command = new SqlCommand($"SELECT AccountID, Username, Password, IsAdmin, LastUpdated FROM {GlobalSettings.AccountTable} WHERE VirtualPcID = @VirtualPcID", connection))
                    {
                        command.Parameters.AddWithValue("@VirtualPcID", _virtualPcId);
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                accounts.Add(new Account
                                {
                                    AccountId = reader.GetGuid(0),
                                    Username = reader.GetString(1),
                                    Password = reader.GetString(2),
                                    IsAdmin = reader.GetBoolean(3), 
                                    LastUpdated = reader.GetDateTime(4).ToString("g") 
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
            public Guid VirtualPcId { get; set; }
            public int CpuCores { get; set; }
            public int RamSizeGb { get; set; }
            public int DiskSizeGb { get; set; }
            public string? VirtualPcName { get; set; }
        }

        public class Account
        {
            public Guid AccountId { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public bool IsAdmin { get; set; }
            public string? LastUpdated { get; set; } 
        }
    }
}
