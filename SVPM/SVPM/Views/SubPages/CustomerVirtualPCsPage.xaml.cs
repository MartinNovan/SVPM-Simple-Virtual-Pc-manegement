using SVPM.Models;
using SVPM.Services;
using SVPM.ViewModels;
using SVPM.Views.CreatingPages;

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
        private async void VirtualPcListView_ItemTapped(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            try
            {
                if (selectionChangedEventArgs.CurrentSelection.FirstOrDefault() is VirtualPc selectedVirtualPc)
                {
                    await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPc));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
