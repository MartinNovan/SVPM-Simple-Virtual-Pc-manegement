using System.Collections.ObjectModel;

namespace SVPM.Pages.CreateRecordsPages;
public partial class CreateVirtualPc
{
    //TODO: Edit this code to be able to use it to editing the customer & fix the picker bug
    public ObservableCollection<Models.Customer> Customers { get; set; } = new (CustomerRepository.CustomersList);

    public ObservableCollection<Models.Customer> SelectedCustomers { get; set; } = new();
    public CreateVirtualPc()
    {
        InitializeComponent();
        BindingContext = this;
    }
    private async void VirtualPcConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(VirtualPcNameEntry.Text))
            {
                await DisplayAlert("Error", "Virtual PC name is required.", "OK");
                return;
            }
            var virtualPc = new Models.VirtualPC
            {
                VirtualPcID = Guid.NewGuid(),
                VirtualPcName = VirtualPcNameEntry.Text,
                ServiceName = ServiceNameEntry.Text,
                OperatingSystem = OperatingSystemEntry.Text,
                CPU_Cores = int.Parse(String.IsNullOrEmpty(CpuCoresEntry.Text) ? "0" : CpuCoresEntry.Text),
                RAM_Size_GB = int.Parse(String.IsNullOrEmpty(RamSizeEntry.Text) ? "0" : RamSizeEntry.Text),
                Disk_Size_GB = int.Parse(String.IsNullOrEmpty(DriveSizeEntry.Text) ? "0" : DriveSizeEntry.Text),
                Backupping = BackuppingCheckBox.IsChecked,
                Administration = AdministrationCheckBox.IsChecked,
                IP_Address = IpAddressEntry.Text,
                FQDN = FqdnEntry.Text,
                Notes = NotesEntry.Text,
                OwningCustomers = SelectedCustomers.ToList(),
                RecordState = Models.RecordStates.Created
            };
            if (SelectedCustomers.Count == 0)
            {
                VirtualPcRepository.VirtualPcsList.Add(virtualPc);
                await DisplayAlert("Success", "Virtual PC successfully added/edited.", "OK");
                await Navigation.PopAsync();
                return;
            }
            foreach (var selectedCustomer in SelectedCustomers)
            {
                var customerVirtualPc = new Models.Mapping()
                {
                    MappingID = Guid.NewGuid(),
                    CustomerID = selectedCustomer.CustomerID,
                    VirtualPcID = virtualPc.VirtualPcID,
                    RecordState = Models.RecordStates.Created
                };
                CustomersVirtualPCsRepository.MappingList.Add(customerVirtualPc);
                virtualPc.RecordState = Models.RecordStates.Updated;
            }
            VirtualPcRepository.VirtualPcsList.Add(virtualPc);
            await DisplayAlert("Success", "Virtual PC successfully added/edited.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void Picker_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            Picker.IsEnabled = false;
            if (Picker.SelectedItem is Models.Customer selectedCustomer)
            {
                SelectedCustomers.Add(selectedCustomer);
                Customers.Remove(selectedCustomer);
                Picker.SelectedIndex = -1; //TODO: Fix this, it freezes the selected item in the picker and picker is not usable after this (usually happens when there is 3 left in the picker)
                await Task.Delay(100);
                Picker.IsEnabled = true;
            }
            Picker.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            Picker.IsEnabled = true;
        }
    }

    private void DeleteSelected(object? sender, EventArgs e)
    {
        if (SelectedCustomersListView.SelectedItem is Models.Customer selectedCustomer)
        {
            SelectedCustomers.Remove(selectedCustomer);
            Customers.Add(selectedCustomer);
        }
    }
}