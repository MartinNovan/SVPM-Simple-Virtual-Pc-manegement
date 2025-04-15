using SVPM.Models;
using SVPM.Repositories;
using SVPM.Views.CreatingPages;

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
            LoadAccounts();
        }
        private async void LoadAccounts()
        {
            try
            {
                AccountsListView.ItemsSource = AccountRepository.Accounts.Where(a => a.AssociatedVirtualPc?.VirtualPcId != null && a.AssociatedVirtualPc.VirtualPcId == _virtualPc.VirtualPcId);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower() ?? "";
            if (String.IsNullOrWhiteSpace(searchText))
            {
                return;
            }
            var match = AccountRepository.Accounts
                .FirstOrDefault(a => a.AssociatedVirtualPc?.VirtualPcId != null && a.AssociatedVirtualPc.VirtualPcId == _virtualPc.VirtualPcId && a.Username!.ToLower().Contains(searchText));
            if (match != null)
            {
                AccountsListView.ScrollTo(match, position: ScrollToPosition.Start, animated: true);
            }
        }
        private async void AddButton_OnClicked(object? sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new CreateAccount());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
