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

    public Guid VirtualPcID { get; init; }

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

    private string? _serviceName;
    public string? ServiceName
    {
        get => _serviceName;
        set
        {
            if (_serviceName != value)
            {
                _serviceName = value;
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

    private int _cpuCores;
    public int CPU_Cores
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

    private int _ramSizeGb;
    public int RAM_Size_GB
    {
        get => _ramSizeGb;
        set
        {
            if (_ramSizeGb != value)
            {
                _ramSizeGb = value;
                NotifyPropertyChanged();
            }
        }
    }

    private int _diskSizeGb;
    public int Disk_Size_GB
    {
        get => _diskSizeGb;
        set
        {
            if (_diskSizeGb != value)
            {
                _diskSizeGb = value;
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
    public string? IP_Address
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
    public string? FQDN
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

    private List<Customer>? _owningCustomers;
    public List<Customer>? OwningCustomers
    {
        get => _owningCustomers;
        set
        {
            if (_owningCustomers != value)
            {
                _owningCustomers = value;
                NotifyPropertyChanged();
            }
        }
    }

    public string OwningCustomersNames => OwningCustomers is { Count: > 0 }
        ? string.Join(", ", OwningCustomers.Select(c => c.FullName))
        : "No customers";

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

    public string? OriginalVirtualPcName { get; private set; }
    public string? OriginalServiceName { get; private set; }
    public string? OriginalOperatingSystem { get; private set; }
    public int OriginalCPU_Cores { get; private set; }
    public int OriginalRAM_Size_GB { get; private set; }
    public int OriginalDisk_Size_GB { get; private set; }
    public bool OriginalBackupping { get; private set; }
    public bool OriginalAdministration { get; private set; }
    public string? OriginalIP_Address { get; private set; }
    public string? OriginalFQDN { get; private set; }
    public string? OriginalNotes { get; private set; }
    public List<Customer>? OriginalOwningCustomers { get; private set; }

    public bool InDatabase { get; set; }

    public void InitializeOriginalValues()
    {
        if (RecordState != RecordStates.Loaded) return;
        OriginalVirtualPcName = VirtualPcName;
        OriginalServiceName = ServiceName;
        OriginalOperatingSystem = OperatingSystem;
        OriginalCPU_Cores = CPU_Cores;
        OriginalRAM_Size_GB = RAM_Size_GB;
        OriginalDisk_Size_GB = Disk_Size_GB;
        OriginalBackupping = Backupping;
        OriginalAdministration = Administration;
        OriginalIP_Address = IP_Address;
        OriginalFQDN = FQDN;
        OriginalNotes = Notes;
        OriginalOwningCustomers = OwningCustomers;
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
