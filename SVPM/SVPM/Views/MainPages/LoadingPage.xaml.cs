using SVPM.Repositories;
using static SVPM.Repositories.CustomerRepository;
using static SVPM.Repositories.CustomersVirtualPCsRepository;
using static SVPM.Repositories.VirtualPcRepository;
using static SVPM.Repositories.AccountRepository;

namespace SVPM.Views.MainPages;

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
        //Navigation.PopAsync();
    }
    private async Task LoadingData()
    {
        try
        {
            Text.Text = "Loading Customers...";
            await GetCustomersAsync();
            ProgressBar.Progress = 0.2;
            Text.Text = "Loading Mappings...";
            await GetAllMappingAsync();
            ProgressBar.Progress = 0.4;
            Text.Text = "Loading Virtual PCs...";
            await GetAllVirtualPCsAsync();
            ProgressBar.Progress = 0.6;
            Text.Text = "Loading Accounts...";
            await GetAllAccountsAsync();
            ProgressBar.Progress = 0.8;
            Text.Text = "Loading Logs...";
            await LogRepository.GetAllLogs();
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
            foreach (var customer in Customers.ToList())
            {
                await customer.SaveChanges();
            }
            ProgressBar.Progress = 0.25;
            Text.Text = "Uploading Virtual PCs changes...";
            foreach (var virtualPc in VirtualPCs.ToList())
            {
                await virtualPc.SaveChanges();
            }
            ProgressBar.Progress = 0.5;
            Text.Text = "Uploading mappings changes...";
            foreach (var mapping in Mappings.ToList())
            {
                await mapping.SaveChanges();
            }
            ProgressBar.Progress = 0.75;
            Text.Text = "Uploading accounts changes...";
            foreach (var account in AccountRepository.Accounts.ToList())
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