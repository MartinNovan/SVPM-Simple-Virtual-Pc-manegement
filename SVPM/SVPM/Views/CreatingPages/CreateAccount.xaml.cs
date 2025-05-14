using SVPM.Models;
using SVPM.Repositories;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.Views.CreatingPages;

public partial class CreateAccount
{
    private static Account? _updatedAccount;
    public CreateAccount(Account? account = null)
    {
        InitializeComponent();
        _updatedAccount = account;
    }
    private void VpcCollectionView_OnLoaded(object? sender, EventArgs e)
    {
        //VpcCollectionView.ItemsSource = VirtualPCs.Where(vpc => vpc.RecordState != RecordStates.Deleted).OrderBy(vpc => vpc.VirtualPcName);
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
            ?.FirstOrDefault(vpc => vpc.VirtualPcId == account!.AssociatedVirtualPc!.VirtualPcId);

        if (itemToSelect != null)
        {
            VpcCollectionView.SelectedItem = itemToSelect;
            VpcCollectionView.ScrollTo(itemToSelect, position: ScrollToPosition.Center, animate: true);
        }
    }
    
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
/*
        var match = VirtualPCs
            .FirstOrDefault(vpc => vpc.VirtualPcName != null && vpc.VirtualPcName.ToLower().Contains(searchText) && vpc.RecordState != RecordStates.Deleted);

        if (match != null)
        {
            VpcCollectionView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
        }
        */
    }
    
    private async void AccountConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            if (VpcCollectionView.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a Virtual PC.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(AccountUsernameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a username.", "OK");
                return;
            }

            var account = new Account
            {
                AccountId = Guid.NewGuid(),
                Username = AccountUsernameEntry.Text,
                Password = AccountPasswordEntry.Text,
                Admin = IsAdminCheckBox.IsChecked,
                Updated = DateTime.Now,
                BackupPassword = AccountOriginalPasswordEntry.Text,
                AssociatedVirtualPc = VpcCollectionView.SelectedItem as VirtualPc,
                RecordState = RecordStates.Created
            };
            account.VerifyHash = CalculateHash.CalculateVerifyHash(null, null, account);
            account.InitializeOriginalValues();
            AccountRepository.Accounts.Add(account);
            await DisplayAlert("Success", "Account successfully added/edited.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}