using SVPM.Models;
using SVPM.Services;
using SVPM.ViewModels;

namespace SVPM.Views.CreatingPages;

public partial class CreateAccount
{
    private static Account? _updatedAccount;
    public CreateAccount(Account? account = null)
    {
        InitializeComponent();
        _updatedAccount = account;
        BindingContext = VirtualPcViewModel.Instance;
        VirtualPcViewModel.Instance.FilterVirtualPCs((SearchBar?.Text ?? "").ToLower());
    }
    private void VpcCollectionView_OnLoaded(object? sender, EventArgs e)
    {
        if (_updatedAccount != null)
        {
            PopulateFields(_updatedAccount);
        }
    }
    
    private void PopulateFields(Account? account)
    {
        AccountUsernameEntry.Text = account?.Username ?? string.Empty;
        AccountPasswordEntry.Text = account?.Password ?? string.Empty;
        AccountOriginalPasswordEntry.Text = account?.BackupPassword ?? string.Empty;
        IsAdminCheckBox.IsChecked = account?.Admin ?? false;
        var itemToSelect = (VpcCollectionView.ItemsSource as IEnumerable<VirtualPc>)
            ?.FirstOrDefault(vpc => vpc.VirtualPcId == account!.VirtualPcId);

        if (itemToSelect != null)
        {
            VpcCollectionView.SelectedItem = itemToSelect;
            VpcCollectionView.ScrollTo(itemToSelect, position: ScrollToPosition.Center, animate: true);
        }
    }
    
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        VirtualPcViewModel.Instance.FilterVirtualPCs(searchText);
    }
    
    private async void AccountConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            if (VpcCollectionView.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a Virtual PC for this account.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(AccountUsernameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a username.", "OK");
                return;
            }

            if (_updatedAccount != null)
            {
                await AccountService.Instance.UpdateAccount(
                    _updatedAccount.AccountId,
                    (VirtualPc)VpcCollectionView.SelectedItem,
                    AccountUsernameEntry.Text,
                    AccountPasswordEntry.Text,
                    AccountOriginalPasswordEntry.Text,
                    IsAdminCheckBox.IsChecked);
                await DisplayAlert("Success", "Account successfully edited.", "OK");
            }
            else
            {
                await AccountService.Instance.CreateAccount(
                    (VirtualPc)VpcCollectionView.SelectedItem,
                    AccountUsernameEntry.Text,
                    AccountPasswordEntry.Text,
                    AccountOriginalPasswordEntry.Text,
                    IsAdminCheckBox.IsChecked);
                await DisplayAlert("Success", "Account successfully added.", "OK");
            }
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}