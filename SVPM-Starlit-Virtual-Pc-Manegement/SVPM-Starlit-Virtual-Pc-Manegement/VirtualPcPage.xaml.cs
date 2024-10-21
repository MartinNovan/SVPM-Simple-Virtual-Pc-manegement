using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class VirtualPcPage : ContentPage
    {
        private List<VirtualPC> virtualPCs;

        public VirtualPcPage()
        {
            InitializeComponent();
            LoadVirtualPCs();
        }

        private async void LoadVirtualPCs()
        {
            try
            {
                string connectionString = GlobalSettings.ConnectionString;
                virtualPCs = new List<VirtualPC>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand($@"SELECT s.ServerID, s.CustomerID, s.CPU_Cores, s.RAM_Size_GB, s.Disk_Size_GB, s.VirtualPcName, c.FullName FROM {GlobalSettings.VirtualPcTable} s INNER JOIN {GlobalSettings.CustomerTable} c ON s.CustomerID = c.CustomerID", connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                virtualPCs.Add(new VirtualPC
                                {
                                    ServerID = reader.GetInt32(0),
                                    CustomerID = reader.GetInt32(1),
                                    CPU_Cores = reader.GetInt32(2),
                                    RAM_Size_GB = reader.GetInt32(3),
                                    Disk_Size_GB = reader.IsDBNull(4) ? 0 : reader.GetInt32(4), // Kontrola na NULL
                                    Virtual_Pc_Name = reader.IsDBNull(5) ? "(Bez Jména)" : reader.GetString(5),
                                    FullName = reader.IsDBNull(6) ? " " : reader.GetString(6) // Vlastník serveru
                                });
                            }
                        }
                    }
                }

                VirtualPCsListView.ItemsSource = virtualPCs;
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
                VirtualPCsListView.ItemsSource = virtualPCs; // Pokud je prázdné, zobrazí všechny
            }
            else
            {
                VirtualPCsListView.ItemsSource = virtualPCs
                    .Where(a => a.Virtual_Pc_Name.ToLower().Contains(searchText) ||
                                 a.FullName.ToLower().Contains(searchText))
                    .ToList();
            }
        }

        public class VirtualPC
        {
            public int ServerID { get; set; }
            public int CustomerID { get; set; }
            public string? FullName { get; set; } // Vlastník serveru
            public int CPU_Cores { get; set; }
            public int RAM_Size_GB { get; set; }
            public int Disk_Size_GB { get; set; }
            public string Virtual_Pc_Name { get; set; }
        }

        private async void VirtualPcListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                var selectedVirtualPC = e.Item as VirtualPC;
                if (selectedVirtualPC != null)
                {
                    await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPC.ServerID));
                }
            }
        }
    }
}
