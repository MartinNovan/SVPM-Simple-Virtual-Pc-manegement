using SVPM.Models;
using SVPM.Repositories;

namespace SVPM.Pages.CreateRecordsPages;
public partial class CreateVirtualPc
{
    private static Models.VirtualPC? _updatedVirtualPc;
    public CreateVirtualPc(Models.VirtualPC? virtualPc = null)
    {
        InitializeComponent();
        _updatedVirtualPc = virtualPc;
    }
    private void CustomerCollectionView_OnLoaded(object? sender, EventArgs e)
    {
        CustomerCollectionView.ItemsSource = CustomerRepository.CustomersList.Where(c => c.RecordState != RecordStates.Deleted).OrderBy(c => c.FullName);
        if (_updatedVirtualPc != null)
        {
            PopulateFields(_updatedVirtualPc);
        }
    }

    private void PopulateFields(Models.VirtualPC? virtualPc)
    {
        VirtualPcNameEntry.Text = virtualPc?.VirtualPcName ?? string.Empty;
        ServiceNameEntry.Text = virtualPc?.ServiceName ?? string.Empty;
        OperatingSystemEntry.Text = virtualPc?.OperatingSystem ?? string.Empty;
        CpuCoresEntry.Text = virtualPc?.CPU_Cores.ToString() ?? string.Empty;
        RamSizeEntry.Text = virtualPc?.RAM_Size_GB.ToString() ?? string.Empty;
        DriveSizeEntry.Text = virtualPc?.Disk_Size_GB.ToString() ?? string.Empty;
        BackuppingCheckBox.IsChecked = virtualPc!.Backupping;
        AdministrationCheckBox.IsChecked = virtualPc!.Administration;
        IpAddressEntry.Text = virtualPc?.IP_Address ?? string.Empty;
        FqdnEntry.Text = virtualPc?.FQDN ?? string.Empty;
        NotesEntry.Text = virtualPc?.Notes ?? string.Empty;
        if (virtualPc?.OwningCustomers != null)
        {
            foreach (var owningCustomer in virtualPc.OwningCustomers)
            {
                CustomerCollectionView.SelectedItems.Add(owningCustomer);
            }
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";

        var match = CustomerRepository.CustomersList
            .FirstOrDefault(c => c.FullName != null && c.FullName.ToLower().Contains(searchText) && c.RecordState != Models.RecordStates.Deleted);

        if (match != null)
        {
            CustomerCollectionView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
        }
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
                var existingVirtualPc = VirtualPcRepository.VirtualPcsList
                    .FirstOrDefault(vpc => vpc.VirtualPcID == _updatedVirtualPc.VirtualPcID);
                if (existingVirtualPc != null)
                {
                    existingVirtualPc.VirtualPcName = VirtualPcNameEntry.Text;
                    existingVirtualPc.ServiceName = ServiceNameEntry.Text;
                    existingVirtualPc.OperatingSystem = OperatingSystemEntry.Text;
                    existingVirtualPc.CPU_Cores = int.Parse(CpuCoresEntry.Text);
                    existingVirtualPc.RAM_Size_GB = int.Parse(RamSizeEntry.Text);
                    existingVirtualPc.Disk_Size_GB = int.Parse(DriveSizeEntry.Text);
                    existingVirtualPc.Backupping = BackuppingCheckBox.IsChecked;
                    existingVirtualPc.Administration = AdministrationCheckBox.IsChecked;
                    existingVirtualPc.IP_Address = IpAddressEntry.Text;
                    existingVirtualPc.FQDN = FqdnEntry.Text;
                    existingVirtualPc.Notes = NotesEntry.Text;
                    existingVirtualPc.RecordState = RecordStates.Updated;

                    if (existingVirtualPc.OwningCustomers != null)
                    {
                        foreach (var customer in CustomerRepository.CustomersList)
                        {
                            if (!CustomerCollectionView.SelectedItems.Contains(customer) && existingVirtualPc.OwningCustomers.Contains(customer))
                            {
                                var deletemapping = CustomersVirtualPCsRepository.MappingList.FirstOrDefault(m =>
                                    m.CustomerID == customer.CustomerID && m.VirtualPcID == existingVirtualPc.VirtualPcID && m.RecordState != Models.RecordStates.Deleted);
                                if (deletemapping != null) deletemapping.RecordState = Models.RecordStates.Deleted;
                                existingVirtualPc.OwningCustomers.Remove(customer);
                            }
                            else if (CustomerCollectionView.SelectedItems.Contains(customer) && !existingVirtualPc.OwningCustomers.Contains(customer))
                            {
                                var customerVirtualPc = new Models.Mapping
                                {
                                    CustomerID = customer.CustomerID,
                                    VirtualPcID = existingVirtualPc.VirtualPcID,
                                    RecordState = Models.RecordStates.Created
                                };
                                CustomersVirtualPCsRepository.MappingList.Add(customerVirtualPc);
                                existingVirtualPc.OwningCustomers.Add(customer);
                            }
                        }
                    }
                    else
                    {
                        var virtualPc = new Models.VirtualPC()
                        {
                            VirtualPcName = VirtualPcNameEntry.Text,
                            ServiceName = ServiceNameEntry.Text,
                            OperatingSystem = OperatingSystemEntry.Text,
                            CPU_Cores = int.Parse(CpuCoresEntry.Text),
                            RAM_Size_GB = int.Parse(RamSizeEntry.Text),
                            Disk_Size_GB = int.Parse(DriveSizeEntry.Text),
                            Backupping = BackuppingCheckBox.IsChecked,
                            Administration = AdministrationCheckBox.IsChecked,
                            IP_Address = IpAddressEntry.Text,
                            FQDN = FqdnEntry.Text,
                            Notes = NotesEntry.Text,
                            RecordState = Models.RecordStates.Created
                        };
                        if (CustomerCollectionView.SelectedItems.Count != 0)
                        {
                            foreach (Customer selectedCustomer in CustomerCollectionView.SelectedItems)
                            {
                                if (selectedCustomer is not null)
                                {
                                    var customerVirtualPc = new Models.Mapping
                                    {
                                        CustomerID = selectedCustomer.CustomerID,
                                        VirtualPcID = virtualPc.VirtualPcID,
                                        RecordState = Models.RecordStates.Created
                                    };
                                    CustomersVirtualPCsRepository.MappingList.Add(customerVirtualPc);
                                    virtualPc.OwningCustomers?.Add(selectedCustomer);
                                }
                            }
                        }
                    }
                }
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