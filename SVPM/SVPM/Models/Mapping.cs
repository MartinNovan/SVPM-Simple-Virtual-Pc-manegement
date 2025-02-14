using SVPM.Repositories;

namespace SVPM.Models;

public class Mapping
{
    public Guid MappingID { get; set; }
    public Guid VirtualPcID { get; set; }
    public Guid CustomerID { get; set; }
    public RecordStates RecordState { get; set; }
    public bool inDatabase { get; set; }
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