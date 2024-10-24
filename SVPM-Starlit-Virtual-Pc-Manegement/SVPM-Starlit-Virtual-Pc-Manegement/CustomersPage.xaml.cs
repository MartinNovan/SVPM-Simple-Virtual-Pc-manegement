using System.Collections.Generic;
using System.Linq; // Přidej tuto direktivu pro LINQ
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;

namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class CustomersPage : ContentPage
    {
        private List<Customer> customers = new List<Customer>(); // Přidej tuto proměnnou

        public CustomersPage()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            try
            {
                string connectionString = GlobalSettings.ConnectionString;
                customers.Clear(); // Vyprázdni seznam před načtením nových zákazníků

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand($"SELECT CustomerID, FullName, Email, Phone FROM {GlobalSettings.CustomerTable}", connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                customers.Add(new Customer
                                {
                                    CustomerID = reader.GetGuid(0),
                                    FullName = reader.GetString(1),
                                    Email = reader.IsDBNull(2) ? " " : reader.GetString(2),
                                    Phone = reader.IsDBNull(3) ? " " : reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            CustomersListView.ItemsSource = customers; // Zobraz všechny zákazníky
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }

        }

        public class Customer
        {
            public Guid CustomerID { get; set; }
            public string? FullName { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                CustomersListView.ItemsSource = customers; // Pokud je prázdné, zobrazí všechny
            }
            else
            {
                CustomersListView.ItemsSource = customers
                    .Where(c => c.FullName.ToLower().Contains(searchText) ||
                                 c.Email.ToLower().Contains(searchText) ||
                                 c.Phone.ToLower().Contains(searchText))
                    .ToList();
            }
        }

        // Při kliknutí na položku uživatele se otevře CustomerServersPage s příslušným CustomerID
        private async void CustomersListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                var selectedCustomer = e.Item as Customer;
                if (selectedCustomer != null)
                {
                    await Navigation.PushAsync(new CustomerVirtualPCsPage(selectedCustomer.CustomerID));
                }
            }
        }
    }
}
