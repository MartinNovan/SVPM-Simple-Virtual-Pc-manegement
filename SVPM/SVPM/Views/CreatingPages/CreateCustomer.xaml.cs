using SVPM.Models;
using SVPM.Services;
using SVPM.ViewModels;
using static SVPM.Repositories.CustomersVirtualPCsRepository;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.Views.CreatingPages;

public partial class CreateCustomer
{
    private static Customer? _updatedCustomer;

    public CreateCustomer(Customer? customer = null)
    {
        InitializeComponent();
        _updatedCustomer = customer;
        BindingContext = VirtualPcViewModel.Instance;
    }

    private void VpcCollectionView_OnLoaded(object? sender, EventArgs e)
    {
        if (_updatedCustomer != null)
        {
            PopulateFields(_updatedCustomer);
        }
    }

    private async void PopulateFields(Customer? customer)
    {
        try
        {
            CustomerFullNameEntry.Text = customer?.FullName ?? string.Empty;
            CustomerTagEntry.Text = customer?.CustomerTag ?? string.Empty;
            CustomerEmailEntry.Text = customer?.Email ?? string.Empty;
            CustomerPhoneEntry.Text = customer?.Phone ?? string.Empty;
            CustomerNotesEntry.Text = customer?.Notes ?? string.Empty;
            var vpcs = await CustomerService.Instance.GetCustomerVirtualPCs(customer);
            VpcCollectionView.SelectedItems.Clear();
            foreach (var vpc in vpcs)
            {
                VpcCollectionView.SelectedItems.Add(vpc);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "There was some error while populating fields: " + ex.Message, "OK");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        /*
        var match = VirtualPCs
            .FirstOrDefault(vpc => vpc.VirtualPcName != null && vpc.VirtualPcName.ToLower().Contains(searchText) && vpc.RecordState != RecordStates.Deleted);

        if (match != null)
        {
            VpcCollectionView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
        }
        */
    }

    private async void CustomerConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CustomerFullNameEntry.Text) ||
                string.IsNullOrWhiteSpace(CustomerTagEntry.Text))
            {
                await DisplayAlert("Error", "Customer full name and tag is required.", "OK");
                return;
            }

            await CustomerService.Instance.CreateCustomer(
                _updatedCustomer?.CustomerId ?? Guid.NewGuid(),
                CustomerFullNameEntry.Text,
                CustomerTagEntry.Text,
                CustomerEmailEntry.Text,
                CustomerPhoneEntry.Text,
                CustomerNotesEntry.Text,
                VpcCollectionView.SelectedItems.Cast<VirtualPc>().ToList()
            );

            if (_updatedCustomer != null)
            {
                await DisplayAlert("Success", "Customer successfully edited.", "OK");
            }
            else
            {
                await DisplayAlert("Success", "Customer successfully added.", "OK");
            }


            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}