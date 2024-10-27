using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.SubWindowPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages
{
    public partial class CustomersPage
    {
        private List<Customer>? _customers;

        public CustomersPage()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            try
            {
                string? connectionString = GlobalSettings.ConnectionString;
                _customers = new List<Customer>();

                await using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    await using (SqlCommand command = new SqlCommand($"SELECT CustomerID, FullName, Email, Phone FROM {GlobalSettings.CustomerTable}", connection))
                    {
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                _customers.Add(new Customer
                                {
                                    CustomerId = reader.GetGuid(0),
                                    FullName = reader.GetString(1),
                                    Email = reader.IsDBNull(2) ? " " : reader.GetString(2),
                                    Phone = reader.IsDBNull(3) ? " " : reader.GetString(3)
                                });
                            }
                        }
                    }
                }
                CustomersListView.ItemsSource = _customers;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", ex.Message, "OK");
            }

        }

        public class Customer
        {
            public Guid CustomerId { get; init; }
            public string? FullName { get; init; }
            public string? Phone { get; init; }
            public string? Email { get; init; }
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string? searchText = e.NewTextValue?.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                CustomersListView.ItemsSource = _customers;
            }
            else
            {
                if (_customers != null)
                    CustomersListView.ItemsSource = _customers
                        .Where(c => c is { FullName: not null, Email: not null, Phone: not null } && 
                                    (c.FullName.ToLower().Contains(searchText) ||
                                    c.Email.ToLower().Contains(searchText) ||
                                    c.Phone.ToLower().Contains(searchText)))
                        .ToList();
            }
        }

        private async void CustomersListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Customer selectedCustomer)
            {
                await Navigation.PushAsync(new CustomerVirtualPCsPage(selectedCustomer.CustomerId));
            }
        }
    }
}
