using SVPM.Models;
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
        CustomerViewModel.Instance.FilterCustomers(SearchBar.Text.ToLower());
    }
    private void CustomerCollectionView_OnLoaded(object? sender, EventArgs e)
    {
        if (_updatedVirtualPc != null)
        {
            PopulateFields(_updatedVirtualPc);
        }
    }

    private void PopulateFields(VirtualPc? virtualPc)
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
        //TODO: Add customer names to the list
        CustomerCollectionView.SelectedItems = null;
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
                /*
                var existingVirtualPc = VirtualPCs
                    .FirstOrDefault(vpc => vpc.VirtualPcId == _updatedVirtualPc.VirtualPcId);
                if (existingVirtualPc != null)
                {
                    existingVirtualPc.VirtualPcName = VirtualPcNameEntry.Text;
                    existingVirtualPc.Service = ServiceNameEntry.Text;
                    existingVirtualPc.OperatingSystem = OperatingSystemEntry.Text;
                    existingVirtualPc.CpuCores = CpuCoresEntry.Text;
                    existingVirtualPc.RamSize = RamSizeEntry.Text;
                    existingVirtualPc.DiskSize = DiskSizeEntry.Text;
                    existingVirtualPc.Backupping = BackuppingCheckBox.IsChecked;
                    existingVirtualPc.Administration = AdministrationCheckBox.IsChecked;
                    existingVirtualPc.IpAddress = IpAddressEntry.Text;
                    existingVirtualPc.Fqdn = FqdnEntry.Text;
                    existingVirtualPc.Notes = NotesEntry.Text;
                    existingVirtualPc.Updated = DateTime.Now;
                    existingVirtualPc.RecordState = RecordStates.Updated;

                    foreach (var customer in Customers)
                    {
                        if (!CustomerCollectionView.SelectedItems.Contains(customer))
                        {
                            var deleteMapping = Mappings.FirstOrDefault(m =>
                                m.CustomerId == customer.CustomerId &&
                                m.VirtualPcId == existingVirtualPc.VirtualPcId &&
                                m.RecordState != RecordStates.Deleted);
                            if (deleteMapping != null) deleteMapping.RecordState = RecordStates.Deleted;
                        }
                        else if (CustomerCollectionView.SelectedItems.Contains(customer))
                        {
                            var customerVirtualPc = new Mapping
                            {
                                CustomerId = customer.CustomerId,
                                VirtualPcId = existingVirtualPc.VirtualPcId,
                                RecordState = RecordStates.Created
                            };
                            Mappings.Add(customerVirtualPc);
                        }
                    }
                    existingVirtualPc.VerifyHash = CalculateHash.CalculateVerifyHash(null, existingVirtualPc);
                }*/
            }
            else
            {
                var virtualPc = new VirtualPc
                {
                    VirtualPcId = Guid.NewGuid(),
                    VirtualPcName = VirtualPcNameEntry.Text,
                    Service = ServiceNameEntry.Text,
                    OperatingSystem = OperatingSystemEntry.Text,
                    CpuCores = CpuCoresEntry.Text,
                    RamSize = RamSizeEntry.Text,
                    DiskSize = DiskSizeEntry.Text,
                    Backupping = BackuppingCheckBox.IsChecked,
                    Administration = AdministrationCheckBox.IsChecked,
                    IpAddress = IpAddressEntry.Text,
                    Fqdn = FqdnEntry.Text,
                    Notes = NotesEntry.Text,
                    Updated = DateTime.Now,
                    RecordState = RecordStates.Created,
                };
                if (CustomerCollectionView.SelectedItems.Count != 0)
                {
                    foreach (Customer selectedCustomer in CustomerCollectionView.SelectedItems)
                    {
                        if (selectedCustomer is not null)
                        {
                            var customerVirtualPc = new Mapping
                            {
                                CustomerId = selectedCustomer.CustomerId,
                                VirtualPcId = virtualPc.VirtualPcId,
                                RecordState = RecordStates.Created
                            };
                            //Mappings.Add(customerVirtualPc);
                        }
                    }
                }
                virtualPc.VerifyHash = CalculateHash.CalculateVerifyHash(null, virtualPc);
                virtualPc.InitializeOriginalValues();
                //VirtualPCs.Add(virtualPc);
            }
            await DisplayAlert("Success", "Virtual PC successfully added/edited.", "OK");
            CustomerCollectionView.ItemsSource = null;
            CustomerCollectionView.SelectedItems = null;
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}