using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;
namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.SubWindowPages
{
    public partial class CustomerVirtualPCsPage
    {
        private readonly Guid _customerId;
        private List<VirtualPc>? _virtualPCs;

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public CustomerVirtualPCsPage(Guid customerId)
        {
            _customerId = customerId;
            InitializeComponent();
            LoadCustomer();
            LoadVirtualPCs();
        }

        private async void LoadVirtualPCs()
        {
            try
            {
                string? connectionString = GlobalSettings.ConnectionString;
                _virtualPCs = new List<VirtualPc>();

                await using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    await using (SqlCommand command = new SqlCommand($"SELECT VirtualPcID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName FROM {GlobalSettings.VirtualPcTable} WHERE CustomerID = @CustomerID", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerID", _customerId);
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                _virtualPCs.Add(new VirtualPc
                                {
                                    VirtualPcId = reader.GetGuid(0),
                                    CpuCores = reader.GetInt32(1),
                                    RamSizeGb = reader.GetInt32(2),
                                    DiskSizeGb = reader.GetInt32(3),
                                    VirtualPcName = reader.IsDBNull(4) ? "(Bez Jména)" : reader.GetString(4)
                                });
                            }
                        }
                    }
                }

                VirtualPCsListView.ItemsSource = _virtualPCs;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }
        }

        private async void LoadCustomer()
        {
            try
            {
                string? connectionString = GlobalSettings.ConnectionString;

                await using SqlConnection connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                await using SqlCommand command = new SqlCommand($"SELECT FullName, Email, Phone FROM {GlobalSettings.CustomerTable} WHERE CustomerID = @CustomerID", connection);
                command.Parameters.AddWithValue("@CustomerID", _customerId);

                await using SqlDataReader reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Načtení dat do CurrentCustomer
                    var customer = new Customer
                    {
                        FullName = reader.GetString(0),
                        Email = reader.GetString(1),
                        Phone = reader.GetString(2)
                    };

                    // Nastavení BindingContext na customer pro FullName
                    BindingContext = customer;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }
        }
        public class VirtualPc
        {
            public Guid VirtualPcId { get; init; }
            public int CpuCores { get; set; }
            public int RamSizeGb { get; set; }
            public int DiskSizeGb { get; set; }
            public string? VirtualPcName { get; set; }
        }

        public class Customer
        {
            public Guid CustomerId { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
        }
        private async void VirtualPcListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is VirtualPc selectedVirtualPc)
            {
                await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPc.VirtualPcId));
            }
        }
    }
}
