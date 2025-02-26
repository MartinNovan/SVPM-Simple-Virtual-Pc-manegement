using SVPM.Models;
using SVPM.Repositories;
using static SVPM.Repositories.AccountRepository;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.Views.CreatingPages;

public partial class CreateAccount
{
    public CreateAccount()
    {
        InitializeComponent();
        VirtualPcPicker.ItemDisplayBinding = new Binding("VirtualPcName");
        VirtualPcPicker.ItemsSource = VirtualPCs.Where(vpc => vpc.RecordState != RecordStates.Deleted).ToList();
    }
    
    private async void AccountConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            if (VirtualPcPicker.SelectedItem == null)
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
                AssociatedVirtualPc = VirtualPcPicker.SelectedItem as VirtualPc,
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