using SVPM.Models;
using SVPM.ViewModels;

namespace SVPM.Services;

public class VirtualPcService
{
    public static VirtualPcService Instance {get;} = new();
    
    public async Task<List<Customer?>> GetVirtualPcCustomers(VirtualPc virtualPc)
    {
        var idList = MappingViewModel.Instance.Mappings.Where(mapping => mapping.VirtualPcId == virtualPc.VirtualPcId || mapping.RecordState != RecordStates.Deleted).Select(mapping => mapping.CustomerId).ToList();
        var customers = new List<Customer?>();
        foreach (var id in idList)
        {
            customers.Add(CustomerViewModel.Instance.SortedCustomers.FirstOrDefault(c => c.CustomerId == id));
        }
        return customers.OrderBy(c =>c?.FullName).ToList();
    }
    
    public async Task RemoveVirtualPc(VirtualPc virtualPc)
    {
        foreach (var mapping in MappingViewModel.Instance.Mappings.Where(m => m.VirtualPcId == virtualPc.VirtualPcId && m.RecordState != RecordStates.Deleted))
        {
            await MappingViewModel.Instance.RemoveMappingAsync(mapping);  
        }
        await VirtualPcViewModel.Instance.RemoveVirtualPc(virtualPc);
    }

    public async Task CreateVirtualPc(string virtualPcName, string? service = null, string? operatingSystem = null,
        string? cpu = null, string? ram = null, string? disk = null ,bool backupping = false, bool administration = false,
        string? ipAddress = null, string? fqdn = null, string? notes = null, List<Customer>? customers = null)
    {

        var virtualPc = new VirtualPc
        {
            VirtualPcId = Guid.NewGuid(),
            VirtualPcName = virtualPcName,
            Service = service,
            OperatingSystem = operatingSystem,
            CpuCores = cpu,
            RamSize = ram,
            DiskSize = disk,
            Backupping = backupping,
            Administration = administration,
            IpAddress = ipAddress,
            Fqdn = fqdn,
            Notes = notes,
            RecordState = RecordStates.Created
        };
        
        virtualPc.VerifyHash = CalculateHash.CalculateVerifyHash(null, virtualPc);
        virtualPc.InitializeOriginalValues();

        if(customers != null && customers.Count != 0) await CreateMappingsForVirtualPc(virtualPc.VirtualPcId, customers);
        
        await VirtualPcViewModel.Instance.SaveVirtualPc(virtualPc);
    }
    
    public async Task UpdateVirtualPc(Guid virtualPcId, string virtualPcName, string? service = null, string? operatingSystem = null,
        string? cpu = null, string? ram = null, string? disk = null ,bool backupping = false, bool administration = false,
        string? ipAddress = null, string? fqdn = null, string? notes = null, List<Customer>? customers = null)
    {
        var virtualPc = VirtualPcViewModel.Instance.SortedVirtualPCs.FirstOrDefault(vpc => vpc.VirtualPcId == virtualPcId);
        if (virtualPc != null)
        {
            virtualPc.VirtualPcName = virtualPcName;
            virtualPc.Service = service;
            virtualPc.OperatingSystem = operatingSystem;
            virtualPc.CpuCores = cpu;
            virtualPc.RamSize = ram;
            virtualPc.DiskSize = disk;
            virtualPc.Backupping = backupping;
            virtualPc.Administration = administration;
            virtualPc.IpAddress = ipAddress;
            virtualPc.Fqdn = fqdn;
            virtualPc.Notes = notes;
            virtualPc.RecordState = RecordStates.Updated;

            if(customers != null && customers.Count != 0) await CreateMappingsForVirtualPc(virtualPc.VirtualPcId, customers);
            
            await VirtualPcViewModel.Instance.SaveVirtualPc(virtualPc);
        }
    }

    private async Task CreateMappingsForVirtualPc(Guid virtualPcId,List<Customer>? customers)
    {
        var mappings = MappingViewModel.Instance.Mappings.Where(m => m.VirtualPcId == virtualPcId).ToList();
        if (customers != null)
        {
            foreach (var c in customers)
            {
                if (mappings.All(m => m.CustomerId != c.CustomerId))
                {
                    var mapping = new Mapping
                    {
                        CustomerId = c.CustomerId,
                        VirtualPcId = virtualPcId,
                        RecordState = RecordStates.Created
                    };
                    await MappingViewModel.Instance.AddMappingAsync(mapping);
                }
            }

            foreach (var mapping in mappings)
            {
                if (customers.All(c => c.CustomerId != mapping.CustomerId))
                {
                    await MappingViewModel.Instance.RemoveMappingAsync(mapping);
                }
            }
        }
    }
}