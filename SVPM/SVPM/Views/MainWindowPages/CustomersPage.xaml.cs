using SVPM.Models;
using SVPM.Views.CreateRecordsPages;
using SVPM.Views.SubWindowPages;
using static SVPM.Repositories.CustomerRepository;
using static SVPM.Repositories.CustomersVirtualPCsRepository;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.Views.MainWindowPages;

public partial class CustomersPage
{
    public CustomersPage()
    {
        InitializeComponent();
        CustomersListView.ItemsSource = Customers;
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return;
        }
        var match = Customers.FirstOrDefault(c => (c.FullName?.ToLower().Contains(searchText) ?? false) ||
                                                      (c.CustomerTag?.ToLower().Contains(searchText) ?? false) ||
                                                      (c.Email?.ToLower().Contains(searchText) ?? false) ||
                                                      (c.Phone?.ToLower().Contains(searchText) ?? false));
        if (match != null)
        {
            CustomersListView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
        }
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

    private async void EditConnection_Clicked(object? sender, EventArgs e)
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

            var confirm = await DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
            if (!confirm) return;

            customer.RecordState = RecordStates.Deleted;
            foreach (var mapping in Mappings.Where(m =>
                         m.CustomerID == customer.CustomerID))
            {
                mapping.RecordState = RecordStates.Deleted;
            }

            foreach (var vpc in VirtualPCs.Where(vpc =>
                         vpc.OwningCustomers != null && vpc.OwningCustomers.Contains(customer)))
            {
                if (vpc.OwningCustomers != null) vpc.OwningCustomers.Remove(customer);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }
}