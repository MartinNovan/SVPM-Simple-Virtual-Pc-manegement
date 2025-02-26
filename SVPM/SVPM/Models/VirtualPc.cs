using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SVPM.Repositories;

namespace SVPM.Models;

public class VirtualPc : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Guid VirtualPcId { get; init; }

    private string? _virtualPcName;
    public string? VirtualPcName
    {
        get => _virtualPcName;
        set
        {
            if (_virtualPcName != value)
            {
                _virtualPcName = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _service;
    public string? Service
    {
        get => _service;
        set
        {
            if (_service != value)
            {
                _service = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _operatingSystem;
    public string? OperatingSystem
    {
        get => _operatingSystem;
        set
        {
            if (_operatingSystem != value)
            {
                _operatingSystem = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _cpuCores;
    public string? CpuCores
    {
        get => _cpuCores;
        set
        {
            if (_cpuCores != value)
            {
                _cpuCores = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _ramSize;
    public string? RamSize
    {
        get => _ramSize;
        set
        {
            if (_ramSize != value)
            {
                _ramSize = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _diskSize;
    public string? DiskSize
    {
        get => _diskSize;
        set
        {
            if (_diskSize != value)
            {
                _diskSize = value;
                NotifyPropertyChanged();
            }
        }
    }

    private bool _backupping;
    public bool Backupping
    {
        get => _backupping;
        set
        {
            if (_backupping != value)
            {
                _backupping = value;
                NotifyPropertyChanged();
            }
        }
    }

    private bool _administration;
    public bool Administration
    {
        get => _administration;
        set
        {
            if (_administration != value)
            {
                _administration = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _ipAddress;
    public string? IpAddress
    {
        get => _ipAddress;
        set
        {
            if (_ipAddress != value)
            {
                _ipAddress = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _fqdn;
    public string? Fqdn
    {
        get => _fqdn;
        set
        {
            if (_fqdn != value)
            {
                _fqdn = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set
        {
            if (_notes != value)
            {
                _notes = value;
                NotifyPropertyChanged();
            }
        }
    }
    private DateTime _updated;
    public DateTime Updated
    {
        get => _updated;
        set
        {
            if (_updated != value)
            {
                _updated = value;
                NotifyPropertyChanged();
            }
        }
    }
    public string? VerifyHash { get; set; }
    private RecordStates _recordState;
    public RecordStates RecordState
    {
        get => _recordState;
        set
        {
            if (_recordState != value)
            {
                _recordState = value;
                NotifyPropertyChanged();
            }
        }
    }

    private ObservableCollection<Customer>? _owningCustomers;
    public ObservableCollection<Customer>? OwningCustomers
    {
        get => _owningCustomers;
        set
        {
            if (_owningCustomers != value)
            {
                _owningCustomers = value;
                SetOwningCustomersNames();
                NotifyPropertyChanged();
            }
        }
    }
    public void SetOwningCustomersNames()
    {
        OwningCustomersNames = OwningCustomers is { Count: > 0 }
            ? string.Join(", ", OwningCustomers.Select(c => c.FullName))
            : "No customers";
    }
    private string? _owningCustomersNames;

    public string? OwningCustomersNames
    {
        get => _owningCustomersNames;
        set
        {
            if (_owningCustomersNames != value)
            {
                _owningCustomersNames = value;
                NotifyPropertyChanged();
            }
        }
    }

    public string? OriginalVirtualPcName { get; private set; }
    public string? OriginalVerifyHash { get; private set; }
    public RecordStates OriginalRecordState { get; private set; }

    public void InitializeOriginalValues()
    {
        OriginalVirtualPcName = VirtualPcName;
        OriginalVerifyHash = VerifyHash;
        OriginalRecordState = RecordState;
    }

    public async Task SaveChanges()
    {
        try
        {
            switch (RecordState)
            {
                case RecordStates.Loaded:
                    break;
                case RecordStates.Created:
                    await VirtualPcRepository.AddVirtualPc(this);
                    break;
                case RecordStates.Deleted:
                    await VirtualPcRepository.DeleteVirtualPc(this);
                    break;
                case RecordStates.Updated:
                    await VirtualPcRepository.UpdateVirtualPc(this);
                    break;
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
