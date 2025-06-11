using SVPM.ViewModels;
using SVPM.Views.MainPages;
using SqlConnection = SVPM.Models.SqlConnection;

namespace SVPM;
//TODO Complete setting page, save unsaved values e.g. to TEMP file, option to automatically (e.g. 5 min intervals) save to database and check changes in database and complete setting JSON file
public partial class AppShell
{
    private int _lastlySelectedItem = -1;
    public AppShell()
    {
        InitializeComponent();
        DispatchDelayed(TimeSpan.FromSeconds(15), CheckForUpdate);
    }

    private void DispatchDelayed(TimeSpan delay, Func<Task> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        Dispatcher.DispatchDelayed(delay, async void () =>
        {
            await action();
        });
    }
    
    private async Task CheckForUpdate()
    {
        try
        {
            bool isNewAvailable = await UpdateChecker.IsNewVersionAvailable();
            if (isNewAvailable)
            {
                await DisplayAlert("Update Available", "A new version of the app is available. Please update to the latest version.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to check for updates: {ex.Message}", "OK");
        }
    }

    private async void SqlPicker_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            if (SqlPicker.SelectedItem is not SqlConnection connection) return;
            await SqlConnectionViewModel.Instance.SelectConnectionAsync(connection);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
        }
    }

    private async void SqlPicker_OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            SqlPicker.ItemsSource = SqlConnectionViewModel.Instance.SortedSqlConnections;
            if(_lastlySelectedItem != -1)  SqlPicker.SelectedIndex = _lastlySelectedItem;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error when loading connections: {ex.Message}", "OK");
        }
    }

    private async void PullFromDatabase(object? sender, EventArgs e)
    {
        try
        {
            if(GlobalSettings.ConnectionString == null) return;
            await Navigation.PushAsync(new LoadingPage(false));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error when pulling from source: {ex.Message}", "OK");
        }
    }

    private async void PushToDatabase(object? sender, EventArgs e)
    {
        try
        {
            if (GlobalSettings.ConnectionString == null) return;
            await Navigation.PushAsync(new LoadingPage(true));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error when pushing to source: {ex.Message}", "OK");
        }
    }
}