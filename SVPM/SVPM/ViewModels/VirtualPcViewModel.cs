using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using SVPM.Models;
using SVPM.Repositories;

namespace SVPM.ViewModels;

public class VirtualPcViewModel
{
    public static VirtualPcViewModel Instance { get; } = new();
    public ObservableCollection<VirtualPc> VirtualPCs { get; } = new();
    public ObservableCollection<VirtualPc> SortedVirtualPCs { get; } = new();
    
    private VirtualPcViewModel()
    {
        VirtualPCs.CollectionChanged += VirtualPCs_CollectionChanged;
    }
    
    private void VirtualPCs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (VirtualPc old in e.OldItems)
                old.PropertyChanged -= VirtualPc_PropertyChanged;

        if (e.NewItems != null)
            foreach (VirtualPc @new in e.NewItems)
                @new.PropertyChanged += VirtualPc_PropertyChanged;

        SortVirtualPCs();
    }
    
    private void VirtualPc_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SortVirtualPCs();
    }
    private void SortVirtualPCs()
    {
        SortedVirtualPCs.Clear();
        foreach (var virtualPc in VirtualPCs.OrderBy(v => v.VirtualPcName))
        {
            if(virtualPc.RecordState != RecordStates.Deleted) SortedVirtualPCs.Add(virtualPc);
        }
    }
    public async Task LoadVirtualPCsAsync()
    {
        var virtualPCs = await VirtualPcRepository.GetAllVirtualPCsAsync();
        
        VirtualPCs.Clear();
        foreach (var virtualPc in virtualPCs)
        {
            VirtualPCs.Add(virtualPc);
        }
    }
    
}