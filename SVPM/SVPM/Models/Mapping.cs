using SVPM.Repositories;

namespace SVPM.Models;

public class Mapping
{
    public Guid MappingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VirtualPcId { get; set; }
    public RecordStates RecordState { get; set; }
    public DateTime Updated { get; set; }
    public bool InDatabase { get; set; }
    public async Task SaveChanges()
    {
        try
        {
            switch (RecordState)
            {
                case RecordStates.Loaded:
                    break;
                case RecordStates.Created:
                    await CustomersVirtualPCsRepository.AddMappingAsync(this);
                    break;
                case RecordStates.Deleted:
                    await CustomersVirtualPCsRepository.DeleteMappingAsync(this);
                    break;
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}