using SVPM.Models;
using SVPM.Views.CreatingPages;
using static SVPM.Repositories.VirtualPcRepository;
//TODO: Change list view to collection view
namespace SVPM.Views.SubPages
{
    public partial class CustomerVirtualPCsPage
    {
        private readonly Customer _customer;
        public CustomerVirtualPCsPage(Customer customer)
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
                VirtualPCsListView.ItemsSource = VirtualPCs.Where(a => a.OwningCustomersNames != null && a.OwningCustomersNames.Contains(_customer.FullName!)).ToList();
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
                VirtualPCsListView.ItemsSource = VirtualPCs;
            }
            else
            {
                if (VirtualPCs != null)
                    VirtualPCsListView.ItemsSource = VirtualPCs
                        .Where(a => a is { VirtualPcName: not null } &&
                                    (a.VirtualPcName.ToLower().Contains(searchText) ||
                                    a.OwningCustomersNames.Contains(_customer.FullName!)))
                        .ToList();
            }
        }

        private async void VirtualPcListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is VirtualPc selectedVirtualPc)
                {
                    await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPc));
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
