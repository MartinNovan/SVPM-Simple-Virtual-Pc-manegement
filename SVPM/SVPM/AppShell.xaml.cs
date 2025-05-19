using SVPM.ViewModels;
using SVPM.Views.MainPages;
using SqlConnection = SVPM.Models.SqlConnection;

namespace SVPM;
//TODO Dodělat setting page, ukládat neuložené hodnoty například to TEMP souboru, možnost automaticky (např. 5min intervali) ukládat do databáze a kontrolovat změny v databázi a dodělat setting JSON soubor
public partial class AppShell
{
    private int _lastlySelectedItem = -1;
    public AppShell()
    {
        InitializeComponent();
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

    private async void CheckBox_OnCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        try
        {
            if (UploadAllCheckBox.IsChecked)
            {
                await DisplayAlert("Warning", "With this setting 'ON' all your local data will be pushed to database regardless if they were or weren't changed!", "OK" );
                Console.WriteLine("Turning on this mode.");
            }
            else
            {
                Console.WriteLine("Turning it off");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error when changing mode: {ex.Message}", "OK");
        }
    }
}