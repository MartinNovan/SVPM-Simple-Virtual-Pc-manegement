using SVPM.Models;
using static SVPM.Repositories.CustomerRepository;
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
    }

    private void VpcCollectionView_OnLoaded(object? sender, EventArgs e)
    {
        VpcCollectionView.ItemsSource = VirtualPCs.Where(vpc => vpc.RecordState != RecordStates.Deleted).OrderBy(vpc => vpc.VirtualPcName);
        if (_updatedCustomer != null)
        {
            PopulateFields(_updatedCustomer);
        }
    }
    private void PopulateFields(Customer? customer)
    {
        CustomerFullNameEntry.Text = customer?.FullName ?? string.Empty;
        CustomerTagEntry.Text = customer?.CustomerTag ?? string.Empty;
        CustomerEmailEntry.Text = customer?.Email ?? string.Empty;
        CustomerPhoneEntry.Text = customer?.Phone ?? string.Empty;
        CustomerNotesEntry.Text = customer?.Notes ?? string.Empty;
        //TODO: Add vpc names to the list
        VpcCollectionView.SelectedItems = null;
    }
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";

        var match = VirtualPCs
            .FirstOrDefault(vpc => vpc.VirtualPcName != null && vpc.VirtualPcName.ToLower().Contains(searchText) && vpc.RecordState != RecordStates.Deleted);

        if (match != null)
        {
            VpcCollectionView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
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

           if (_updatedCustomer != null)
           {
               var existingCustomer = Customers
                   .FirstOrDefault(c => c.CustomerId == _updatedCustomer.CustomerId);

               if (existingCustomer != null)
               {
                   existingCustomer.FullName = CustomerFullNameEntry.Text;
                   existingCustomer.CustomerTag = CustomerTagEntry.Text;
                   existingCustomer.Email = CustomerEmailEntry.Text;
                   existingCustomer.Phone = CustomerPhoneEntry.Text;
                   existingCustomer.Notes = CustomerNotesEntry.Text;
                   existingCustomer.VerifyHash = CalculateHash.CalculateVerifyHash(existingCustomer);
                   existingCustomer.Updated = DateTime.Now;
                   existingCustomer.RecordState = RecordStates.Updated;

                   foreach (var vpc in VirtualPCs)
                   {
                       if (VpcCollectionView.SelectedItems.Contains(vpc) && vpc.OwningCustomers != null && !vpc.OwningCustomers.Contains(existingCustomer))
                       {
                           var customerVirtualPc = new Mapping
                           {
                               CustomerId = existingCustomer.CustomerId,
                               VirtualPcId = vpc.VirtualPcId,
                               RecordState = RecordStates.Created
                           };

                           Mappings.Add(customerVirtualPc);
                       }
                       else if (!VpcCollectionView.SelectedItems.Contains(vpc) && vpc.OwningCustomers != null && vpc.OwningCustomers.Contains(existingCustomer))
                       {
                           var deleteMapping = Mappings.FirstOrDefault(m =>
                               m.CustomerId == existingCustomer.CustomerId && m.VirtualPcId == vpc.VirtualPcId && m.RecordState != RecordStates.Deleted);
                           if (deleteMapping != null) deleteMapping.RecordState = RecordStates.Deleted;
                       }
                   }
               }
           }
           else
           {
               var customer = new Customer
               {
                   CustomerId = Guid.NewGuid(),
                   FullName = CustomerFullNameEntry.Text,
                   CustomerTag = CustomerTagEntry.Text,
                   Email = CustomerEmailEntry.Text,
                   Phone = CustomerPhoneEntry.Text,
                   Notes = CustomerNotesEntry.Text,
                   RecordState = RecordStates.Created
               };
               customer.VerifyHash = CalculateHash.CalculateVerifyHash(customer);
               customer.InitializeOriginalValues();
               Customers.Add(customer);
               if (VpcCollectionView.SelectedItems.Count == 0)
               {
                   await DisplayAlert("Success", "Customer successfully added/edited.", "OK");
                   await Navigation.PopAsync();
                   return;
               }
               foreach (var item in VpcCollectionView.SelectedItems)
               {
                   if (item is VirtualPc selectedVirtualPc)
                   {
                       var customerVirtualPc = new Mapping
                       {
                           CustomerId = customer.CustomerId,
                           VirtualPcId = selectedVirtualPc.VirtualPcId,
                           RecordState = RecordStates.Created
                       };

                       Mappings.Add(customerVirtualPc);
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
}