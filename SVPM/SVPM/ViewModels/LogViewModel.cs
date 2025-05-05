using System.Collections.ObjectModel;
using SVPM.Models;
using SVPM.Repositories;

namespace SVPM.ViewModels;

public class CustomerLogViewModel
{
    public static CustomerLogViewModel Instance { get; } = new();
    private ObservableCollection<CustomerLog> CustomersLogs { get; } = new();
    public ObservableCollection<CustomerLog> SortedCustomersLogs { get; } = new();

    private CustomerLogViewModel()
    {
        CustomersLogs.CollectionChanged += (_, _) => SortLogs();
    }

    private void SortLogs()
    {
        SortedCustomersLogs.Clear();
        foreach (var log in CustomersLogs.OrderByDescending(log => log.Updated))
        {
            SortedCustomersLogs.Add(log);
        }
    }
    
    public async Task LoadLogsAsync()
    {
        var logs = await LogRepository.GetCustomerLogs();

        CustomersLogs.Clear();
        foreach (var log in logs)
        {
            CustomersLogs.Add(log);
        }
    } 
}
public class VirtualPcLogViewModel
{
    public static VirtualPcLogViewModel Instance { get; } = new();
    private ObservableCollection<VirtualPcLog> VirtualPcsLogs { get; } = new();
    public ObservableCollection<VirtualPcLog> SortedVirtualPcsLogs { get; } = new();

    private VirtualPcLogViewModel()
    {
        VirtualPcsLogs.CollectionChanged += (_, _) => SortLogs();
    }
    private void SortLogs()
    {
        SortedVirtualPcsLogs.Clear();
        foreach (var log in VirtualPcsLogs.OrderByDescending(log => log.Updated))
        {
            SortedVirtualPcsLogs.Add(log);
        }
    }
    public async Task LoadLogsAsync()
    {
        var logs = await LogRepository.GetVirtualPcLogs();

        VirtualPcsLogs.Clear();
        foreach (var log in logs)
        {
            VirtualPcsLogs.Add(log);
        }
    }
}

public class MappingLogViewModel
{
    public static MappingLogViewModel Instance { get; } = new();
    private ObservableCollection<MappingLog> MappingsLogs { get; set; } = new();
    public ObservableCollection<MappingLog> SortedMappingsLogs { get; } = new();

    private MappingLogViewModel()
    {
        MappingsLogs.CollectionChanged += (_, _) => SortLogs();
    }
    private void SortLogs()
    {
        SortedMappingsLogs.Clear();
        foreach (var log in MappingsLogs.OrderByDescending(log => log.Updated))
        {
            SortedMappingsLogs.Add(log);
        }
    }
    public async Task LoadLogsAsync()
    {
        var logs = await LogRepository.GetMappingLogs();

        MappingsLogs.Clear();
        foreach (var log in logs)
        {
            MappingsLogs.Add(log);
        }
    }
}

public class AccountLogViewModel
{
    public static AccountLogViewModel Instance { get; } = new();
    private ObservableCollection<AccountLog> AccountsLogs { get; set; } = new();
    public ObservableCollection<AccountLog> SortedAccountsLogs { get; } = new();

    private AccountLogViewModel()
    {
        AccountsLogs.CollectionChanged += (_, _) => SortLogs();
    }
    private void SortLogs()
    {
        SortedAccountsLogs.Clear();
        foreach (var log in AccountsLogs.OrderByDescending(log => log.Updated))
        {
            SortedAccountsLogs.Add(log);
        }
    }
    public async Task LoadLogsAsync()
    {
        var logs = await LogRepository.GetAccountsLogs();

        AccountsLogs.Clear();
        foreach (var log in logs)
        {
            AccountsLogs.Add(log);
        }
    }
}