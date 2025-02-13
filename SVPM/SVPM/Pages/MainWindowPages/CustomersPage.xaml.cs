using SVPM.Pages.CreateRecordsPages;
using SVPM.Pages.SubWindowPages;

namespace SVPM.Pages.MainWindowPages
{
    public partial class CustomersPage
    {
        public CustomersPage()
        {
            InitializeComponent();
        }

        private void CustomersListView_OnLoaded(object? sender, EventArgs e)
        {
            CustomersListView.ItemsSource = CustomerRepository.CustomersList.Where(customer => customer.RecordState != Models.RecordStates.Deleted).OrderBy(c => c.FullName);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CustomersListView.ItemsSource = null;
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower() ?? "";
            if (String.IsNullOrWhiteSpace(searchText))
            {
                return;
            }
            var match = CustomerRepository.CustomersList
            .FirstOrDefault(c => (c.FullName?.ToLower().Contains(searchText) ?? false) ||
                         (c.CustomerTag?.ToLower().Contains(searchText) ?? false) ||
                         (c.Email?.ToLower().Contains(searchText) ?? false) ||
                         (c.Phone?.ToLower().Contains(searchText) ?? false) &&
                         c.RecordState != Models.RecordStates.Deleted);
            if (match != null)
            {
                CustomersListView.ScrollTo(match, position: ScrollToPosition.Start, animated: true);
            }
        }

        private async void CustomersListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is Models.Customer selectedCustomer)
                {
                    await Navigation.PushAsync(new CustomerVirtualPCsPage(selectedCustomer));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void AddButton_OnClicked(object? sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new CreateCustomer());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void EditConnection_Clicked(object? sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not Models.Customer customer) return;
                await Navigation.PushAsync(new CreateCustomer(customer));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnDeleteButtonClicked(object? sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not Models.Customer customer) return;

                bool confirm = await DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
                if (!confirm) return;

                customer.RecordState = Models.RecordStates.Deleted;
                foreach (var mapping in CustomersVirtualPCsRepository.MappingList)
                {
                    if (mapping.CustomerID == customer.CustomerID)
                    {
                        mapping.RecordState = Models.RecordStates.Deleted;
                    }
                }

                foreach (var vitrualpc in VirtualPcRepository.VirtualPcsList)
                {
                    if (vitrualpc.OwningCustomers.Contains(customer))
                    {
                        vitrualpc.OwningCustomers.Remove(customer);
                    }
                }
                CustomersListView.ItemsSource = CustomerRepository.CustomersList.Where(customer => customer.RecordState != Models.RecordStates.Deleted).OrderBy(c => c.FullName);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
            }
        }
    }
}
