using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.SubWindowPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages
{
    public partial class VirtualPcPage
    {
        private List<VirtualPc>? _virtualPCs;

        public VirtualPcPage()
        {
            InitializeComponent();
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
                    await using (SqlCommand command = new SqlCommand($@"SELECT s.VirtualPcID, s.CustomerID, s.CPU_Cores, s.RAM_Size_GB, s.Disk_Size_GB, s.VirtualPcName, c.FullName FROM {GlobalSettings.VirtualPcTable} s INNER JOIN {GlobalSettings.CustomerTable} c ON s.CustomerID = c.CustomerID", connection))
                    {
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                _virtualPCs.Add(new VirtualPc
                                {
                                    VirtualPcId = reader.GetGuid(0),
                                    CustomerId = reader.GetGuid(1),
                                    CpuCores = reader.GetInt32(2),
                                    RamSizeGb = reader.GetInt32(3),
                                    DiskSizeGb = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    VirtualPcName = reader.IsDBNull(5) ? "(Bez Jména)" : reader.GetString(5),
                                    FullName = reader.IsDBNull(6) ? " " : reader.GetString(6)
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

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string? searchText = e.NewTextValue?.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                VirtualPCsListView.ItemsSource = _virtualPCs;
            }
            else
            {
                if (_virtualPCs != null)
                    VirtualPCsListView.ItemsSource = _virtualPCs
                        .Where(a => a is { FullName: not null, VirtualPcName: not null } && 
                                    (a.VirtualPcName.ToLower().Contains(searchText) ||
                                    a.FullName.ToLower().Contains(searchText)))
                        .ToList();
            }
        }

        public class VirtualPc
        {
            public Guid VirtualPcId { get; init; }
            public Guid CustomerId { get; set; }
            public string? FullName { get; init; }
            public int CpuCores { get; set; }
            public int RamSizeGb { get; set; }
            public int DiskSizeGb { get; set; }
            public string? VirtualPcName { get; init; }
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
