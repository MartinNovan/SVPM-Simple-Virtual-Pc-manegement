namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages;

public partial class LoadingPage
{
    public LoadingPage(bool isPushing)
    {
        InitializeComponent();
        Text.Text = "Loading...";
        if (isPushing)
        {
            PushingData();
        }
        else
        {
            LoadingData();
        }
    }

    private async void LoadingData()
    {
        try
        {
            Text.Text = "Loading Customers...";
            await CustomerRepository.GetAllCustomersAsync();
            ProgressBar.Progress = 0.25;
            Text.Text = "Loading Mappings...";
            await CustomersVirtualPCsRepository.GetAllMappingAsync();
            ProgressBar.Progress = 0.5;
            Text.Text = "Loading Virtual PCs...";
            await VirtualPcRepository.GetAllVirtualPCsAsync();
            ProgressBar.Progress = 0.75;
            Text.Text = "Loading Accounts...";
            await AccountRepository.GetAllAccountsAsync();
            ProgressBar.Progress = 1;
            Text.Text = "Done!";
            await Navigation.PushAsync(new MainTabbedPage());
            Navigation.RemovePage(this);
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void PushingData()
    {
        try
        {
            //TODO: Finish uploading data to database with verification (more in models.cs TODO)
            Text.Text = "Uploading customers changes...";
            foreach (var customer in CustomerRepository.CustomersList.Where(c => c.RecordState != Models.RecordStates.Loaded))
            {
                customer.SaveChanges();
            }
            ProgressBar.Progress = 0.25;
            Text.Text = "Uploading mappings changes...";
            foreach (var mapping in CustomersVirtualPCsRepository.MappingList.Where(m => m.RecordState != Models.RecordStates.Loaded))
            {
                mapping.SaveChanges();
            }
            ProgressBar.Progress = 0.5;
            Text.Text = "Uploading Virtual PCs changes...";
            foreach (var virtualPc in VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Loaded))
            {
                virtualPc.SaveChanges();
            }
            ProgressBar.Progress = 0.75;
            Text.Text = "Uploading accounts changes...";
            foreach (var account in VirtualPcRepository.VirtualPcsList.Where(acc => acc.RecordState != Models.RecordStates.Loaded))
            {
                account.SaveChanges();
            }
            ProgressBar.Progress = 1;
            Text.Text = "Done!";
            await Navigation.PushAsync(new MainTabbedPage());
            Navigation.RemovePage(this);
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}