using SVPM.Models;
using SVPM.Views.CreatingPages;
using SVPM.Views.SubPages;
using static SVPM.Repositories.CustomerRepository;
using static SVPM.Repositories.CustomersVirtualPCsRepository;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.Views.MainPages;

public partial class CustomersPage
{
    public CustomersPage()
    {
        InitializeComponent();
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

            var confirm = await DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
            if (!confirm) return;

            customer.RecordState = RecordStates.Deleted;
            foreach (var mapping in Mappings.Where(m =>
                         m.CustomerId == customer.CustomerId))
            {
                mapping.RecordState = RecordStates.Deleted;
            }

            foreach (var vpc in VirtualPCs.Where(vpc =>
                         vpc.OwningCustomers != null && vpc.OwningCustomers.Contains(customer)))
            {
                if (vpc.OwningCustomers != null) vpc.OwningCustomers.Remove(customer);
            }

            if (customer.OriginalRecordState != RecordStates.Loaded)
            {
                Customers.Remove(customer);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }

    private void CustomersListView_OnLoaded(object? sender, EventArgs e)
    {
        CustomersListView.ItemsSource = Customers;
    }
}