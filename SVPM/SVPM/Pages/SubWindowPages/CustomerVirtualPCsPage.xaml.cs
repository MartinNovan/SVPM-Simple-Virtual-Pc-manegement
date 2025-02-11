using SVPM.Pages.CreateRecordsPages;

namespace SVPM.Pages.SubWindowPages
{
    public partial class CustomerVirtualPCsPage
    {
        private readonly Models.Customer _customer;
        public CustomerVirtualPCsPage(Models.Customer customer)
        {
            _customer = customer;
            InitializeComponent();
            BindingContext = _customer;
            LoadVirtualPCs();
        }
        private async void LoadVirtualPCs()
        {
            try
            {
                VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(a => a.OwningCustomersNames != null && a.OwningCustomersNames.Contains(_customer.FullName!)).ToList();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string? searchText = e.NewTextValue?.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList;
            }
            else
            {
                if (VirtualPcRepository.VirtualPcsList != null)
                    VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList
                        .Where(a => a is { OwningCustomersNames: not null, VirtualPcName: not null } &&
                                    (a.VirtualPcName.ToLower().Contains(searchText) ||
                                    a.OwningCustomersNames.Contains(_customer.FullName!)))
                        .ToList();
            }
        }

        private async void VirtualPcListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is Models.VirtualPC selectedVirtualPc)
                {
                    await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPc.VirtualPcID));
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
                await Navigation.PushAsync(new CreateVirtualPc());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void ReloadButton_OnClicked(object? sender, EventArgs e)
        {
            LoadVirtualPCs();
        }
    }
}
