namespace SVPM.Pages.MainWindowPages;

public partial class LoadingPage
{
    public LoadingPage(bool isPushing, bool isFromConnectionPage)
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
            if (isFromConnectionPage)
            {
                Navigation.PushAsync(new MainTabbedPage());
            }
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
            Navigation.RemovePage(this);
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
            await UpdateTextAsync("Uploading customers changes...");
            foreach (var customer in CustomerRepository.CustomersList)
            {
                await customer.SaveChanges();
            }
            await UpdateProgressAsync(0.25);
            await UpdateTextAsync("Uploading Virtual PCs changes...");
            foreach (var virtualPc in VirtualPcRepository.VirtualPcsList)
            {
                await virtualPc.SaveChanges();
            }
            await UpdateProgressAsync(0.5);
            await UpdateTextAsync("Uploading mappings changes...");
            foreach (var mapping in CustomersVirtualPCsRepository.MappingList)
            {
                await mapping.SaveChanges();
            }
            await UpdateProgressAsync(0.75);
            await UpdateTextAsync("Uploading accounts changes...");
            foreach (var account in AccountRepository.AccountsList)
            {
                await account.SaveChanges();
            }
            await UpdateProgressAsync(1);
            await UpdateTextAsync("Done!");
            Navigation.RemovePage(this);
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error while pushing data to database: {ex.Message}", "OK");
        }
    }

    private Task UpdateTextAsync(string text)
    {
        return Device.InvokeOnMainThreadAsync(() => Text.Text = text);
    }

    private Task UpdateProgressAsync(double progress)
    {
        return Device.InvokeOnMainThreadAsync(() => ProgressBar.Progress = progress);
    }
}