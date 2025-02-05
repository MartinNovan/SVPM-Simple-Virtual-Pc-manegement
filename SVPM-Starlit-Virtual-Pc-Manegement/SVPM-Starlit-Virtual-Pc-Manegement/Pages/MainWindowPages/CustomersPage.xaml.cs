using SVPM_Starlit_Virtual_Pc_Manegement.Pages.CreateRecordsPages;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.SubWindowPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages
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
            string? searchText = e.NewTextValue?.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                CustomersListView.ItemsSource = CustomerRepository.CustomersList.Where(customer => customer.RecordState != Models.RecordStates.Deleted).OrderBy(c => c.FullName);
            }
            else
            {
                CustomersListView.ItemsSource = CustomerRepository.CustomersList
                    .Where(c => c is { FullName: not null,CustomerTag:not null, Email: not null, Phone: not null } &&
                                (c.FullName.ToLower().Contains(searchText) ||
                                 c.CustomerTag.ToLower().Contains(searchText) ||
                                 c.Email.ToLower().Contains(searchText) ||
                                 c.Phone.ToLower().Contains(searchText)) && c.RecordState != Models.RecordStates.Deleted)
                    .OrderBy(c => c.FullName);
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

        private void ReloadButton_OnClicked(object? sender, EventArgs e)
        {
            CustomersListView.ItemsSource = CustomerRepository.CustomersList.Where(customer => customer.RecordState != Models.RecordStates.Deleted).OrderBy(c => c.FullName);
        }

        private void EditConnection_Clicked(object? sender, EventArgs e)
        {
            Navigation.PushAsync(new CreateCustomer());
        }

        private async void OnDeleteButtonClicked(object? sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not Models.Customer customer) return;

                bool confirm = await DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
                if (!confirm) return;

                customer.RecordState = Models.RecordStates.Deleted;
                CustomersListView.ItemsSource = CustomerRepository.CustomersList.Where(customer => customer.RecordState != Models.RecordStates.Deleted).OrderBy(c => c.FullName);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
            }
        }
    }
}
