using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SVPM.Models;

using static SVPM.Repositories.CustomersVirtualPCsRepository;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.ViewModels;

public class CustomerViewModel : INotifyPropertyChanged
{
    public ICommand DeleteCustomerCommand { get; }

    public CustomerViewModel()
    {
        DeleteCustomerCommand = new Command<Customer>(async void (customer) => await DeleteCustomer(customer));
    }

    private async Task DeleteCustomer(Customer customer)
    {
        try
        {
            var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert("Warning!", "Do you really want to delete this customer?", "OK", "Cancel");
            if (!confirm) return;

            customer.RecordState = RecordStates.Deleted;
            foreach (var mapping in Mappings.Where(m =>
                         m.CustomerID == customer.CustomerID))
            {
                mapping.RecordState = RecordStates.Deleted;
            }

            foreach (var vpc in VirtualPCs.Where(vpc =>
                         vpc.OwningCustomers != null && vpc.OwningCustomers.Contains(customer)))
            {
                if (vpc.OwningCustomers != null) vpc.OwningCustomers.Remove(customer);
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Failed to delete customer: {ex.Message}", "OK");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}