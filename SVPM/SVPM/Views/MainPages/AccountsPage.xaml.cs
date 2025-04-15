using SVPM.Models;
using SVPM.Repositories;
using SVPM.Views.CreatingPages;

namespace SVPM.Views.MainPages;
//TODO: Change list view to collection view
public partial class AccountsPage
{
    public AccountsPage()
    {
        InitializeComponent();
    }
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        if (String.IsNullOrWhiteSpace(searchText))
        {
            return;
        }
        var match = AccountRepository.Accounts
            .FirstOrDefault(a => a.Username?.ToLower().Contains(searchText) ?? false);
        if (match != null)
        {
            AccountsListView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
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

    private void EditButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is not ImageButton { BindingContext: Account account } || account.RecordState == RecordStates.Deleted) return;
        Navigation.PushAsync(new CreateAccount(account));
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ImageButton button || button.BindingContext is not Account account) return;

            bool confirm = await DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
            if (!confirm) return;
            account.RecordState = RecordStates.Deleted;
            if (account.OriginalRecordState != RecordStates.Loaded)
            {
                AccountRepository.Accounts.Remove(account);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }

    private void AccountsListView_OnLoaded(object? sender, EventArgs e)
    {
        AccountsListView.ItemsSource = AccountRepository.Accounts;
    }
}