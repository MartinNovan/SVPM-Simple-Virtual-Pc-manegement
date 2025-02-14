using SVPM.Repositories;

namespace SVPM.Models;

public class VirtualPc
{
    public Guid VirtualPcID { get; init; }
    public string? VirtualPcName { get; set; }
    public string? ServiceName { get; set; }
    public string? OperatingSystem { get; set; }
    public int CPU_Cores { get; set; }
    public int RAM_Size_GB { get; set; }
    public int Disk_Size_GB { get; set; }
    public bool Backupping { get; set; }
    public bool Administration { get; set; }
    public string? IP_Address { get; set; }
    public string? FQDN { get; set; }
    public string? Notes { get; set; }
    public List<Customer>? OwningCustomers { get; set; }

    public string OwningCustomersNames => OwningCustomers is { Count: > 0 }
        ? string.Join(", ", OwningCustomers.Select(c => c.FullName))
        : "No customers";

    public RecordStates RecordState { get; set; }

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
    public List<Customer>? OriginalOwningCustomers { get; set; }


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