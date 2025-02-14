using SVPM.Repositories;

namespace SVPM.Models;

public class VirtualPC
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
    public string OwningCustomersNames => OwningCustomers is { Count: > 0 } ? string.Join(", ", OwningCustomers.Select(c => c.FullName)) : "No customers";
    public RecordStates RecordState { get; set; }
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
                    await VirtualPcRepository.DeleteVirtualPc(VirtualPcID);
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