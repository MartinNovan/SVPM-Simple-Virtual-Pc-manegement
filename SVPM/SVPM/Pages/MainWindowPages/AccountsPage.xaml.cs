using SVPM.Pages.CreateRecordsPages;

namespace SVPM.Pages.MainWindowPages
{
    public partial class AccountsPage
    {
        public AccountsPage()
        {
            InitializeComponent();
        }

        private void AccountsListView_OnLoaded(object? sender, EventArgs e)
        {
            AccountsListView.ItemsSource = AccountRepository.AccountsList.Where(account => account.RecordState != Models.RecordStates.Deleted).OrderBy(a => a.Username);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            AccountsListView.ItemsSource = null;
        }
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                AccountsListView.ItemsSource = AccountRepository.AccountsList.Where(account => account.RecordState != Models.RecordStates.Deleted).OrderBy(a => a.Username);
            }
            else
            {
                AccountsListView.ItemsSource = AccountRepository.AccountsList
                    .Where(a => a is { Username: not null, VirtualPcName: not null} &&
                                (a.Username.ToLower().Contains(searchText) ||
                                 a.VirtualPcName.ToLower().Contains(searchText)) && a.RecordState != Models.RecordStates.Deleted)
                    .ToList().OrderBy(a => a.Username);
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

        private void ReloadButton_OnClicked(object? sender, EventArgs e)
        {
            AccountsListView.ItemsSource = AccountRepository.AccountsList.Where(account => account.RecordState != Models.RecordStates.Deleted).OrderBy(a => a.Username);
        }
        private void EditConnection_Clicked(object? sender, EventArgs e)
        {
            Navigation.PushAsync(new CreateAccount());
        }

        private async void OnDeleteButtonClicked(object? sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not Models.Account account) return;

                bool confirm = await DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
                if (!confirm) return;

                account.RecordState = Models.RecordStates.Deleted;
                AccountsListView.ItemsSource = AccountRepository.AccountsList.Where(acc => acc.RecordState != Models.RecordStates.Deleted).OrderBy(acc => acc.Username);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
            }
        }
    }
}
