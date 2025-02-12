namespace SVPM.Pages.CreateRecordsPages;
public partial class CreateCustomer
{
    //TODO: Edit this code to be able to use it to editing the customer
    public CreateCustomer()
    {
        InitializeComponent();
        VpcCollectionView.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Deleted);
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
           if (VpcCollectionView.SelectedItems.Count == 0)
           {
               await DisplayAlert("Success", "Customer successfully added/edited.", "OK");
               await Navigation.PopAsync();
               return;
           }
           foreach (var item in VpcCollectionView.SelectedItems)
           {
               if (item is Models.VirtualPC selectedVirtualPc)
               {
                   var customerVirtualPc = new Models.Mapping
                   {
                       CustomerID = customer.CustomerID,
                       VirtualPcID = selectedVirtualPc.VirtualPcID,
                       RecordState = Models.RecordStates.Created
                   };

                   CustomersVirtualPCsRepository.MappingList.Add(customerVirtualPc);
                   selectedVirtualPc.OwningCustomers?.Add(customer);
               }
           }
           await DisplayAlert("Success", "Customer successfully added/edited.", "OK");
           VpcCollectionView.ItemsSource = null;
           VpcCollectionView.SelectedItems = null;
           await Navigation.PopAsync();
       }
       catch (Exception ex)
       {
           await DisplayAlert("Error", ex.Message, "OK");
       }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";

        var match = VirtualPcRepository.VirtualPcsList
            .FirstOrDefault(vpc => vpc.VirtualPcName.ToLower().Contains(searchText) && vpc.RecordState != Models.RecordStates.Deleted);

        if (match != null)
        {
            VpcCollectionView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
        }
    }
}