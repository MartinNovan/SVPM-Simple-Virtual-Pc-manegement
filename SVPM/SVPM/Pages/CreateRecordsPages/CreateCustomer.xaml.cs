using System.Collections.ObjectModel;

namespace SVPM.Pages.CreateRecordsPages;
public partial class CreateCustomer
{
    //TODO: Edit this code to be able to use it to editing the customer & fix the picker bug
    public ObservableCollection<Models.VirtualPC>? VirtualPcs { get; set; } = VirtualPcRepository.VirtualPcsList;

    public ObservableCollection<Models.VirtualPC> SelectedVirtualPcs { get; set; } = new();
    public CreateCustomer()
    {
        InitializeComponent();
        BindingContext = this;
    }
    private async void CustomerConfirmClicked(object sender, EventArgs e)
    {
       try
       {
           if (string.IsNullOrWhiteSpace(CustomerFullNameEntry.Text) || string.IsNullOrWhiteSpace(CustomerTagEntry.Text))
           {
               await DisplayAlert("Error", "Customer full name and tag is required.", "OK");
               return;
           }
           var customer = new Models.Customer
           {
               CustomerID = Guid.NewGuid(),
               FullName = CustomerFullNameEntry.Text,
               CustomerTag = CustomerTagEntry.Text,
               Email = CustomerEmailEntry.Text,
               Phone = CustomerPhoneEntry.Text,
               Notes = CustomerNotesEntry.Text,
               RecordState = Models.RecordStates.Created
           };
           CustomerRepository.CustomersList.Add(customer);
           if (SelectedVirtualPcs.Count == 0)
           {
               await DisplayAlert("Success", "Customer successfully added/edited.", "OK");
               await Navigation.PopAsync();
               return;
           }
           foreach (var selectedVirtualPc in SelectedVirtualPcs)
           {
                var customerVirtualPc = new Models.Mapping()
                {
                     MappingID = Guid.NewGuid(),
                     CustomerID = customer.CustomerID,
                     VirtualPcID = selectedVirtualPc.VirtualPcID,
                     RecordState = Models.RecordStates.Created
                };
                CustomersVirtualPCsRepository.MappingList.Add(customerVirtualPc);
                selectedVirtualPc.OwningCustomers?.Add(customer);
                selectedVirtualPc.RecordState = Models.RecordStates.Updated;
           }
           await DisplayAlert("Success", "Customer successfully added/edited.", "OK");
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
            Picker.SelectedIndexChanged -= Picker_OnSelectedIndexChanged;
            Picker.IsEnabled = false;

            if (Picker.SelectedItem is Models.VirtualPC selectedVirtualPc)
            {
                Console.WriteLine($"Selected VirtualPC: {selectedVirtualPc}");
                if (!SelectedVirtualPcs.Contains(selectedVirtualPc))
                {
                    SelectedVirtualPcs.Add(selectedVirtualPc);
                }
                Picker.SelectedIndex = -1;
                Picker.ItemsSource = VirtualPcs;
            }

            Picker.IsEnabled = true;
            Picker.SelectedIndexChanged += Picker_OnSelectedIndexChanged;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            Picker.IsEnabled = true;
            Picker.SelectedIndexChanged += Picker_OnSelectedIndexChanged;
        }
    }


    private void DeleteSelected(object? sender, EventArgs e)
    {
        if (SelectedVirtualPcsListView.SelectedItem is Models.VirtualPC selectedVirtualPc)
        {
            SelectedVirtualPcs.Remove(selectedVirtualPc);
        }
    }
}