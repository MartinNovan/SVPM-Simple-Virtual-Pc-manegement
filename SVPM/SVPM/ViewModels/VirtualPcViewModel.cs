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
    private void SortVirtualPCs(string searchText = "")
    {
        SortedVirtualPCs.Clear();
        if (String.IsNullOrEmpty(searchText))
        {
            foreach (var virtualPc in VirtualPCs.OrderBy(vpc => vpc.VirtualPcName))
            {
                if(virtualPc.RecordState != RecordStates.Deleted) SortedVirtualPCs.Add(virtualPc);
            }
            return;
        }
        
        searchText = searchText.ToLower();
        var filtered = VirtualPCs
            .Where(vpc =>
                (vpc.VirtualPcName != null && vpc.VirtualPcName.ToLower().Contains(searchText)) ||
                (vpc.Fqdn != null && vpc.Fqdn.ToLower().Contains(searchText)) ||
                (vpc.IpAddress != null && vpc.IpAddress.ToLower().Contains(searchText)) ||
                (vpc.OperatingSystem != null && vpc.OperatingSystem.ToLower().Contains(searchText)) ||
                (vpc.Service != null && vpc.Service.ToLower().Contains(searchText)))
            .Where(vpc => vpc.RecordState != RecordStates.Deleted)
            .OrderBy(vpc => vpc.VirtualPcName);

        foreach (var virtualpc in filtered)
        {
            SortedVirtualPCs.Add(virtualpc);
        }
    }
    
    public void FilterVirtualPCs(string searchText)
    {
        SortVirtualPCs(searchText);
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
    
    public void RemoveVirtualPc(VirtualPc virtualPc)
    {
        virtualPc.RecordState = RecordStates.Deleted;
        if (virtualPc.OriginalRecordState != RecordStates.Loaded)
        {
            VirtualPCs.Remove(virtualPc);
        }
    }

    public Task SaveVirtualPc(VirtualPc virtualPc)
    {
        var match = VirtualPCs.FirstOrDefault(vpc => vpc.VirtualPcId == virtualPc.VirtualPcId);
        if (match != null)
        {
            VirtualPCs.Remove(match);
            virtualPc.RecordState = RecordStates.Updated;
        }
        else
        {
            virtualPc.RecordState = RecordStates.Created;
        }
        VirtualPCs.Add(virtualPc);
        return Task.CompletedTask;
    }
    
    public async Task UploadChanges()
    {
        foreach (var virtualPc in VirtualPCs.Where(c => c.RecordState != RecordStates.Loaded).OrderBy(c => c.RecordState == RecordStates.Deleted ? 0 :
                     c.RecordState == RecordStates.Created ? 1 :
                     c.RecordState == RecordStates.Updated ? 2 : 3).ToList())
        {
            switch (virtualPc.RecordState)
            {
                case RecordStates.Created:
                    await VirtualPcRepository.AddVirtualPc(virtualPc);
                    virtualPc.RecordState = RecordStates.Loaded;
                    virtualPc.InitializeOriginalValues();
                    break;
                case RecordStates.Updated:
                    if (virtualPc.OriginalRecordState != RecordStates.Loaded) { await VirtualPcRepository.AddVirtualPc(virtualPc); return; }
                    await VirtualPcRepository.UpdateVirtualPc(virtualPc);
                    virtualPc.RecordState = RecordStates.Loaded;
                    virtualPc.InitializeOriginalValues();
                    break;
                case RecordStates.Deleted:
                    if (virtualPc.OriginalRecordState != RecordStates.Loaded){ VirtualPCs.Remove(virtualPc); return;}
                    await VirtualPcRepository.DeleteVirtualPc(virtualPc);
                    VirtualPCs.Remove(virtualPc);
                    break;
            }
        }
    }
}