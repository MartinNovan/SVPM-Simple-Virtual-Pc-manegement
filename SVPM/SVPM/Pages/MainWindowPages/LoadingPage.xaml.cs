namespace SVPM.Pages.MainWindowPages;

public partial class LoadingPage
{
    private readonly bool _pushing;
    public LoadingPage(bool pushing)
    {
        InitializeComponent();
        _pushing = pushing;
    }
    private void LoadingPage_OnLoaded(object? sender, EventArgs e)
    {
        if (_pushing)
        {
            PushingData();
        }
        else
        {
            LoadingData();
        }
        Navigation.PopAsync();
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
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error while loading data from database: {ex.Message}", "OK");
        }
    }

    private async void PushingData()
    {
        try
        {
            Text.Text = "Uploading customers changes...";
            foreach (var customer in CustomerRepository.CustomersList)
            {
                await customer.SaveChanges();
            }
            ProgressBar.Progress = 0.25;
            Text.Text = "Uploading Virtual PCs changes...";
            foreach (var virtualPc in VirtualPcRepository.VirtualPcsList)
            {
                await virtualPc.SaveChanges();
            }
            ProgressBar.Progress = 0.5;
            Text.Text = "Uploading mappings changes...";
            foreach (var mapping in CustomersVirtualPCsRepository.MappingList)
            {
                await mapping.SaveChanges();
            }
            ProgressBar.Progress = 0.75;
            Text.Text = "Uploading accounts changes...";
            foreach (var account in AccountRepository.AccountsList)
            {
                await account.SaveChanges();
            }
            ProgressBar.Progress = 1;
            Text.Text = "Done!";
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error while pushing data to database: {ex.Message}", "OK");
        }
    }
}