using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.IdentityModel.Tokens;
using SVPM.Models;
using SVPM.Repositories;

namespace SVPM.ViewModels;

public class CustomerViewModel
{
    public static CustomerViewModel Instance {get;} = new();
    private ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Customer> SortedCustomers { get; } = new();

    private CustomerViewModel()
    {
        Customers.CollectionChanged += Customers_CollectionChanged;
    }

    private void Customers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (Customer old in e.OldItems)
                old.PropertyChanged -= Customer_PropertyChanged;

        if (e.NewItems != null)
            foreach (Customer @new in e.NewItems)
                @new.PropertyChanged += Customer_PropertyChanged;

        SortCustomers();
    }

    private void Customer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SortCustomers();
    }
    private void SortCustomers(string searchText = "")
    {
        SortedCustomers.Clear();
        if (string.IsNullOrEmpty(searchText))
        {
            foreach (var customer in Customers.OrderBy(c => c.CustomerTag))
            {
                if(customer.RecordState != RecordStates.Deleted) SortedCustomers.Add(customer);
            }
            return;
        }
        
        searchText = searchText.ToLower();
        var filtered = Customers
            .Where(c =>
                (c.CustomerTag != null && c.CustomerTag.ToLower().Contains(searchText)) ||
                (c.FullName != null && c.FullName.ToLower().Contains(searchText)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchText)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(searchText)))
            .Where(c => c.RecordState != RecordStates.Deleted)
            .OrderBy(c => c.CustomerTag);

        foreach (var customer in filtered)
        {
            SortedCustomers.Add(customer);
        }

    }
    
    public void FilterCustomers(string searchText)
    { 
        SortCustomers(searchText);
    }
    
    public async Task LoadCustomersAsync()
    {
        var customers = await CustomerRepository.GetCustomersAsync();
        
        Customers.Clear();
        foreach (var customer in customers)
        {
            Customers.Add(customer);
        }
    }
    
    public async Task RemoveCustomer(Customer customer)
    {
        customer.RecordState = RecordStates.Deleted;
        if (customer.OriginalRecordState != RecordStates.Loaded)
        {
            Customers.Remove(customer);
        }
    }

    public async Task SaveCustomer(Customer customer)
    {
        var match = Customers.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
        if (match != null)
        {
            Customers.Remove(match);
            customer.RecordState = RecordStates.Updated;
        }
        else
        {
            customer.RecordState = RecordStates.Created;
        }
        Customers.Add(customer);
    }
    
    public async Task UploadChanges()
    {
        foreach (var customer in Customers.Where(c => c.RecordState != RecordStates.Loaded).OrderBy(c => c.RecordState == RecordStates.Deleted ? 0 :
                     c.RecordState == RecordStates.Created ? 1 :
                     c.RecordState == RecordStates.Updated ? 2 : 3).ToList())
        {
            switch (customer.RecordState)
            {
                case RecordStates.Created:
                    await CustomerRepository.AddCustomer(customer);
                    customer.RecordState = RecordStates.Loaded;
                    customer.InitializeOriginalValues();
                    break;
                case RecordStates.Updated:
                    if (customer.OriginalRecordState != RecordStates.Loaded) { await CustomerRepository.AddCustomer(customer); return; }
                    await CustomerRepository.UpdateCustomer(customer);
                    customer.RecordState = RecordStates.Loaded;
                    customer.InitializeOriginalValues();
                    break;
                case RecordStates.Deleted:
                    if (customer.OriginalRecordState != RecordStates.Loaded){ Customers.Remove(customer); return;}
                    await CustomerRepository.DeleteCustomer(customer);
                    Customers.Remove(customer);
                    break;
            }
        }
    }
}