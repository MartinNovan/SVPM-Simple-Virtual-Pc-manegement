namespace SVPM.Pages.CreateRecordsPages;
public partial class CreateCustomer
{
    //TODO: Edit this code to be able to use it to editing the customer
    private static Models.Customer? UpdatedCustomer = null;
    public CreateCustomer(Models.Customer? customer = null)
    {
        InitializeComponent();
        VpcCollectionView.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Deleted);
        UpdatedCustomer = customer;
        if (UpdatedCustomer != null)
        {
            PopulateFields(UpdatedCustomer);
        }
    }
    private void PopulateFields(Models.Customer? customer)
    {
        CustomerFullNameEntry.Text = customer?.FullName ?? string.Empty;
        CustomerTagEntry.Text = customer?.CustomerTag ?? string.Empty;
        CustomerEmailEntry.Text = customer?.Email ?? string.Empty;
        CustomerPhoneEntry.Text = customer?.Phone ?? string.Empty;
        CustomerNotesEntry.Text = customer?.Notes ?? string.Empty;
        var selectedVirtualPcs = VirtualPcRepository.VirtualPcsList
            .Where(vpc => vpc.OwningCustomers != null && vpc.OwningCustomers.Contains(customer)).ToList();
        foreach (var vpc in selectedVirtualPcs)
        {
            VpcCollectionView.SelectedItems.Add(vpc);
        }
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

           if (UpdatedCustomer != null)
           {
               var existingCustomer = CustomerRepository.CustomersList
                   .FirstOrDefault(c => c.CustomerID == UpdatedCustomer.CustomerID);

               if (existingCustomer != null)
               {
                   existingCustomer.FullName = CustomerFullNameEntry.Text;
                   existingCustomer.CustomerTag = CustomerTagEntry.Text;
                   existingCustomer.Email = CustomerEmailEntry.Text;
                   existingCustomer.Phone = CustomerPhoneEntry.Text;
                   existingCustomer.Notes = CustomerNotesEntry.Text;
                   existingCustomer.RecordState = Models.RecordStates.Updated;

                   foreach (var item in VpcCollectionView.SelectedItems)
                   {
                       if (item is Models.VirtualPC selectedVirtualPc && !selectedVirtualPc.OwningCustomers.Contains(existingCustomer))
                       {
                           var customerVirtualPc = new Models.Mapping
                           {
                               CustomerID = existingCustomer.CustomerID,
                               VirtualPcID = selectedVirtualPc.VirtualPcID,
                               RecordState = Models.RecordStates.Created
                           };

                           CustomersVirtualPCsRepository.MappingList.Add(customerVirtualPc);
                           selectedVirtualPc.OwningCustomers?.Add(existingCustomer);
                       }
                   }
               }
           }
           else
           {
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