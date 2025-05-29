using SVPM.Models;
using SVPM.Services;
using SVPM.ViewModels;

namespace SVPM.Views.CreatingPages;
public partial class CreateVirtualPc
{
    private static VirtualPc? _updatedVirtualPc;
    
    public CreateVirtualPc(VirtualPc? virtualPc = null)
    {
        InitializeComponent();
        _updatedVirtualPc = virtualPc;
        BindingContext = CustomerViewModel.Instance;
        CustomerViewModel.Instance.FilterCustomers((SearchBar?.Text ?? "").ToLower());
    }
    private void CustomerCollectionView_OnLoaded(object? sender, EventArgs e)
    {
        if (_updatedVirtualPc != null)
        {
            PopulateFields(_updatedVirtualPc);
        }
    }

    private async void PopulateFields(VirtualPc? virtualPc)
    {
        try
        {
            VirtualPcNameEntry.Text = virtualPc?.VirtualPcName ?? string.Empty;
            ServiceNameEntry.Text = virtualPc?.Service ?? string.Empty;
            OperatingSystemEntry.Text = virtualPc?.OperatingSystem ?? string.Empty;
            CpuCoresEntry.Text = virtualPc?.CpuCores ?? string.Empty;
            RamSizeEntry.Text = virtualPc?.RamSize ?? string.Empty;
            DiskSizeEntry.Text = virtualPc?.DiskSize ?? string.Empty;
            BackuppingCheckBox.IsChecked = virtualPc!.Backupping;
            AdministrationCheckBox.IsChecked = virtualPc.Administration;
            IpAddressEntry.Text = virtualPc.IpAddress ?? string.Empty;
            FqdnEntry.Text = virtualPc.Fqdn ?? string.Empty;
            NotesEntry.Text = virtualPc.Notes ?? string.Empty;
            var customers = await VirtualPcService.Instance.GetVirtualPcCustomers(virtualPc);
            CustomerCollectionView.SelectedItems.Clear();
            foreach (var customer in customers)
            {
                CustomerCollectionView.SelectedItems.Add(customer);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "There was an error while populating fields: " + ex.Message, "OK");
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        CustomerViewModel.Instance.FilterCustomers(searchText);
    }
    private async void VirtualPcConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(VirtualPcNameEntry.Text))
            {
                await DisplayAlert("Error", "Virtual PC name is required.", "OK");
                return;
            }

            if (_updatedVirtualPc != null)
            {
                await VirtualPcService.Instance.UpdateVirtualPc(
                    _updatedVirtualPc.VirtualPcId,
                    VirtualPcNameEntry.Text,
                    ServiceNameEntry.Text,
                    OperatingSystemEntry.Text,
                    CpuCoresEntry.Text,
                    RamSizeEntry.Text,
                    DiskSizeEntry.Text,
                    BackuppingCheckBox.IsChecked,
                    AdministrationCheckBox.IsChecked,
                    IpAddressEntry.Text,
                    FqdnEntry.Text,
                    NotesEntry.Text,
                    CustomerCollectionView.SelectedItems.Cast<Customer>().ToList()
                );
                await DisplayAlert("Success", "Virtual PC successfully edited.", "OK");
            }
            else
            {
                await VirtualPcService.Instance.CreateVirtualPc(
                    VirtualPcNameEntry.Text,
                    ServiceNameEntry.Text,
                    OperatingSystemEntry.Text,
                    CpuCoresEntry.Text,
                    RamSizeEntry.Text,
                    DiskSizeEntry.Text,
                    BackuppingCheckBox.IsChecked,
                    AdministrationCheckBox.IsChecked,
                    IpAddressEntry.Text,
                    FqdnEntry.Text,
                    NotesEntry.Text,
                    CustomerCollectionView.SelectedItems.Cast<Customer>().ToList()
                );
                await DisplayAlert("Success", "Virtual PC successfully added.", "OK");
            }
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}