using System.Collections.ObjectModel;
using SVPM.Models;
using SVPM.Repositories;

namespace SVPM.ViewModels;

public class MappingViewModel
{
    public static MappingViewModel Instance { get; } = new();
    public ObservableCollection<Mapping> Mappings { get; } = new();

    private MappingViewModel()
    {
        
    }
    
    public async Task LoadMappingsAsync()
    {
        var mappings = await CustomersVirtualPCsRepository.GetAllMappingAsync();
        Mappings.Clear();
        foreach (var mapping in mappings)
        {
            Mappings.Add(mapping);
        }
    }
    
    public async Task AddMappingAsync(Mapping mapping)
    {
        mapping.RecordState = RecordStates.Created;
        Mappings.Add(mapping);
    }
    
    public async Task RemoveMappingAsync(Mapping mapping)
    {
        if(!mapping.InDatabase) {Mappings.Remove(mapping); return;}
        mapping.RecordState = RecordStates.Deleted;
    }

    public async Task UploadChanges()
    {
        foreach (var mapping in Mappings.ToList())
        {
            switch (mapping.RecordState)
            {
                case RecordStates.Created:
                    await CustomersVirtualPCsRepository.AddMappingAsync(mapping);
                    mapping.RecordState = RecordStates.Loaded;
                    mapping.InDatabase = true;
                    break;
                case RecordStates.Deleted:
                    if (!mapping.InDatabase){ Mappings.Remove(mapping); return;}
                    await CustomersVirtualPCsRepository.DeleteMappingAsync(mapping);
                    Mappings.Remove(mapping);
                    break;
            }
        }
    }
}