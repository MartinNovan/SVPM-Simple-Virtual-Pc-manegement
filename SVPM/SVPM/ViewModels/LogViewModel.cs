using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SVPM.Models;

namespace SVPM.ViewModels;

public class CustomerLogViewModel : INotifyPropertyChanged
{
    public static ObservableCollection<CustomerLog> CustomersLogs { get; } = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
public class VirtualPcLogViewModel : INotifyPropertyChanged
{
    public static ObservableCollection<VirtualPcLog> VirtualPcsLogs { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class MappingLogViewModel : INotifyPropertyChanged
{
    public static ObservableCollection<MappingLog> MappingsLogs { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class AccountLogViewModel : INotifyPropertyChanged
{
    public static ObservableCollection<AccountLog> AccountsLogs { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}