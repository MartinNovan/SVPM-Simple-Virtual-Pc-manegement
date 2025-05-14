using SVPM.Models;
using SVPM.ViewModels;
using SVPM.Views.CreatingPages;
using SVPM.Views.SubPages;
using static SVPM.Repositories.CustomersVirtualPCsRepository;

namespace SVPM.Views.MainPages;

public partial class CustomersPage
{
    public CustomersPage()
    {
        InitializeComponent();
        BindingContext = CustomerViewModel.Instance;
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return;
        }/*
        var match = Customers.FirstOrDefault(c => (c.FullName?.ToLower().Contains(searchText) ?? false) ||
                                                      (c.CustomerTag?.ToLower().Contains(searchText) ?? false) ||
                                                      (c.Email?.ToLower().Contains(searchText) ?? false) ||
                                                      (c.Phone?.ToLower().Contains(searchText) ?? false));
        if (match != null)
        {
            CustomersListView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
        }*/
    }

    private async void AddButton_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new CreateCustomer());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void CustomersListView_ItemTapped(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        try
        {
            if (selectionChangedEventArgs.CurrentSelection.FirstOrDefault() is Customer selectedCustomer)
            {
                await Navigation.PushAsync(new CustomerVirtualPCsPage(selectedCustomer));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void EditButton_Clicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ImageButton { BindingContext: Customer customer } || customer.RecordState == RecordStates.Deleted) return;
            await Navigation.PushAsync(new CreateCustomer(customer));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ImageButton { BindingContext: Customer customer }) return;

            
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }
}