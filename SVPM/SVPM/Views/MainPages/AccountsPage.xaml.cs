using SVPM.Models;
using SVPM.ViewModels;
using SVPM.Views.CreatingPages;

namespace SVPM.Views.MainPages;
public partial class AccountsPage
{
    public AccountsPage()
    {
        InitializeComponent();
        BindingContext = AccountViewModel.Instance;
        AccountViewModel.Instance.FilterAccounts((SearchBar?.Text ?? "").ToLower());
    }
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        AccountViewModel.Instance.FilterAccounts(searchText);
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
            await AccountViewModel.Instance.RemoveAccount(account);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }
}