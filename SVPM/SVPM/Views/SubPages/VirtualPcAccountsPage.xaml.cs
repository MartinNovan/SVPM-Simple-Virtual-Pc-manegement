using SVPM.Models;
using SVPM.Services;

namespace SVPM.Views.SubPages
{
    //TODO: Change list view to collection view
    public partial class VirtualPcAccountsPage
    {
        private readonly VirtualPc _virtualPc;

        public VirtualPcAccountsPage(VirtualPc virtualPc)
        {
            InitializeComponent();
            _virtualPc = virtualPc;
            BindingContext = _virtualPc;
            Init();
        }
        private async void Init()
        {
            try
            {
                AccountsListView.ItemsSource = await VirtualPcService.Instance.GetVirtualPcAccounts(_virtualPc);
                Customers.Text = await VirtualPcService.Instance.GetCustomerNamesAsString(_virtualPc);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
