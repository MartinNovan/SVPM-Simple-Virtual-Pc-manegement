using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class CustomerVirtualPCsPage : ContentPage
    {
        private int _customerId;

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public CustomerVirtualPCsPage(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            LoadCustomer();
            LoadVirtualPCs(); 
        }

        private async void LoadVirtualPCs()
        {
            try
            {
                string connectionString = GlobalSettings.ConnectionString;
                List<VirtualPC> virtualPCs = new List<VirtualPC>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand($"SELECT ServerID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName FROM {GlobalSettings.VirtualPcTable} WHERE CustomerID = @CustomerID", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerID", _customerId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                virtualPCs.Add(new VirtualPC
                                {
                                    ServerID = reader.GetInt32(0),
                                    CPU_Cores = reader.GetInt32(1),
                                    RAM_Size_GB = reader.GetInt32(2),
                                    Disk_Size_GB = reader.GetInt32(3),
                                    Virtual_Pc_Name = reader.IsDBNull(4) ? "(Bez Jména)" : reader.GetString(4)
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

        private async void LoadCustomer()
        {
            try
            {
                string connectionString = GlobalSettings.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand($"SELECT FullName, Email, Phone FROM {GlobalSettings.CustomerTable} WHERE CustomerID = @CustomerID", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerID", _customerId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
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
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }
        }
        public class VirtualPC
        {
            public int ServerID { get; set; }
            public int CPU_Cores { get; set; }
            public int RAM_Size_GB { get; set; }
            public int Disk_Size_GB { get; set; }
            public string Virtual_Pc_Name { get; set; }
        }

        public class Customer
        {
            public int CustomerID { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
        private async void VirtualPcListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                var selectedVirtualPC= e.Item as VirtualPC;
                if (selectedVirtualPC != null)
                {
                    await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPC.ServerID));
                }
            }
        }
    }
}
