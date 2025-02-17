using SVPM.Models;
using SVPM.Views.CreateRecordsPages;
using static SVPM.Repositories.AccountRepository;

namespace SVPM.Views.MainWindowPages;
//TODO: Change list view to collection view
public partial class AccountsPage
{
    public AccountsPage()
    {
        InitializeComponent();
        AccountsListView.ItemsSource = AccountsList;
    }
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        if (String.IsNullOrWhiteSpace(searchText))
        {
            return;
        }
        var match = AccountsList
            .FirstOrDefault(a => a.Username?.ToLower().Contains(searchText) ?? false);
        if (match != null)
        {
            AccountsListView.ScrollTo(match, position: ScrollToPosition.Start, animated: true);
        }
    }
    private async void AddButton_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new CreateAccount());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void EditConnection_Clicked(object? sender, EventArgs e)
    {
        Navigation.PushAsync(new CreateAccount());
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ImageButton button || button.BindingContext is not Account account) return;

            bool confirm = await DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
            if (!confirm) return;
            account.RecordState = RecordStates.Deleted;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }
}