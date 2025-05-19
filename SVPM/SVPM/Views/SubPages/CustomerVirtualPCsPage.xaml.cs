using SVPM.Models;
using SVPM.Services;
using SVPM.ViewModels;
using SVPM.Views.CreatingPages;

namespace SVPM.Views.SubPages
{
    public partial class CustomerVirtualPCsPage
    {
        public readonly Customer _customer;
        public CustomerVirtualPCsPage(Customer customer)
        {
            _customer = customer;
            InitializeComponent();
            BindingContext = _customer;
            VirtualPCsListView.BindingContext = CustomerViewModel.Instance;
            LoadVirtualPCs();
        }
        private async void LoadVirtualPCs()
        {
            try
            {
                var virtualPCs = await CustomerService.Instance.GetCustomerVirtualPCs(_customer);
                VirtualPCsListView.ItemsSource = virtualPCs;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string? searchText = e.NewTextValue?.ToLower();
/*
            if (string.IsNullOrWhiteSpace(searchText))
            {
                VirtualPCsListView.ItemsSource = VirtualPCs;
            }
            else
            {
                VirtualPCsListView.ItemsSource = VirtualPCs
                    .Where(a => a.VirtualPcName != null &&
                                a.VirtualPcName.ToLower().Contains(searchText));
            }
            */
        }

        private async void VirtualPcListView_ItemTapped(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            try
            {
                if (sender is VirtualPc selectedVirtualPc)
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

    }
}
