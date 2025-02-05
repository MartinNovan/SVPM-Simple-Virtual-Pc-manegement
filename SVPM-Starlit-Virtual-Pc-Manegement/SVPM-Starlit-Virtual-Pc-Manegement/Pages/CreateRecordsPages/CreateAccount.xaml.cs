namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.CreateRecordsPages;

public partial class CreateAccount
{
    public CreateAccount()
    {
        InitializeComponent();
        VirtualPcPicker.ItemDisplayBinding = new Binding("VirtualPcName");
        VirtualPcPicker.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Deleted).ToList();
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

            var account = new Models.Account
            {
                Username = AccountUsernameEntry.Text,
                Password = AccountPasswordEntry.Text,
                IsAdmin = IsAdminCheckBox.IsChecked,
                LastUpdated = DateTime.Now,
                OriginalPassword = AccountOriginalPasswordEntry.Text,
                AssociatedVirtualPc = VirtualPcPicker.SelectedItem as Models.VirtualPC,
                RecordState = Models.RecordStates.Created
            };

            AccountRepository.AccountsList.Add(account);
            await DisplayAlert("Success", "Account successfully added/edited.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}