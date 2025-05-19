using SVPM.Models;
using SVPM.ViewModels;

namespace SVPM.Services;

public class CustomerService
{
    public static CustomerService Instance {get;} = new();
    private CustomerService()
    {
    }

    public async Task<List<VirtualPc?>> GetCustomerVirtualPCs(Customer customer)
    {
        var idList = MappingViewModel.Instance.Mappings.Where(mapping => mapping.CustomerId == customer.CustomerId).Select(mapping => mapping.VirtualPcId).ToList();
        var virtualPCs = new List<VirtualPc?>();
        foreach (var id in idList)
        {
            virtualPCs.Add(VirtualPcViewModel.Instance.VirtualPCs.FirstOrDefault(vpc => vpc.VirtualPcId == id));
        }
        return virtualPCs.OrderBy(vpc =>vpc?.VirtualPcName).ToList();
    }
    
    public async Task RemoveCustomer(Customer customer)
    {
        foreach (var mapping in MappingViewModel.Instance.Mappings.Where(m => m.CustomerId == customer.CustomerId && m.RecordState != RecordStates.Deleted))
        {
            await MappingViewModel.Instance.RemoveMappingAsync(mapping);  
        }
        await CustomerViewModel.Instance.RemoveCustomer(customer);
    }

    public async Task CreateCustomer(Guid customerId, string fullname, string customerTag, string? email = null,
        string? phone = null, string? notes = null, List<VirtualPc>? virtualPcs = null)
    {

        var customer = new Customer
        {
            CustomerId = customerId,
            FullName = fullname,
            CustomerTag = customerTag,
            Email = email,
            Phone = phone,
            Notes = notes,
            RecordState = RecordStates.Created
        };
        
        customer.VerifyHash = CalculateHash.CalculateVerifyHash(customer);
        customer.InitializeOriginalValues();

        if(virtualPcs != null && virtualPcs.Count != 0) await CreateMappingsForCustomer(customer.CustomerId, virtualPcs);
        
        await CustomerViewModel.Instance.SaveCustomer(customer);
    }
    
    public async Task UpdateCustomer(Guid customerId, string fullname, string customerTag, string? email = null,
        string? phone = null, string? notes = null, List<VirtualPc>? virtualPcs = null)
    {
        var customer = CustomerViewModel.Instance.SortedCustomers.FirstOrDefault(c => c.CustomerId == customerId);
        if (customer != null)
        {
            customer.FullName = fullname;
            customer.CustomerTag = customerTag;
            customer.Email = email;
            customer.Phone = phone;
            customer.Notes = notes;
            customer.RecordState = RecordStates.Updated;

            if(virtualPcs != null && virtualPcs.Count != 0) await CreateMappingsForCustomer(customer.CustomerId, virtualPcs);
            
            await CustomerViewModel.Instance.SaveCustomer(customer);
        }
    }

    private async Task CreateMappingsForCustomer(Guid customerId,List<VirtualPc>? virtualPcs)
    {
        var mappings = MappingViewModel.Instance.Mappings.Where(m => m.CustomerId == customerId).ToList();
        if (virtualPcs != null)
        {
            foreach (var vpc in virtualPcs)
            {
                if (mappings.All(m => m.VirtualPcId != vpc.VirtualPcId))
                {
                    var mapping = new Mapping
                    {
                        CustomerId = customerId,
                        VirtualPcId = vpc.VirtualPcId,
                        RecordState = RecordStates.Created
                    };
                    await MappingViewModel.Instance.AddMappingAsync(mapping);
                }
            }

            foreach (var mapping in mappings)
            {
                if (virtualPcs.All(vpc => vpc.VirtualPcId != mapping.VirtualPcId))
                {
                    await MappingViewModel.Instance.RemoveMappingAsync(mapping);
                }
            }
        }
    }
}
