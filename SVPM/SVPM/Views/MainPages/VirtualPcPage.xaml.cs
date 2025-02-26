using SVPM.Models;
using SVPM.Views.CreatingPages;
using SVPM.Views.SubPages;
using static SVPM.Repositories.CustomersVirtualPCsRepository;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.Views.MainPages;
public partial class VirtualPcPage
{
    public VirtualPcPage()
    {
        InitializeComponent();
        VirtualPCsListView.ItemsSource = VirtualPCs;
    }
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        if (String.IsNullOrWhiteSpace(searchText))
        {
            return;
        }
        var match = VirtualPCs
            .FirstOrDefault(vpc => (vpc.VirtualPcName?.ToLower().Contains(searchText) ?? false) &&
                                 vpc.RecordState != RecordStates.Deleted);
        if (match != null)
        {
            VirtualPCsListView.ScrollTo(match, position: ScrollToPosition.Start, animate: true);
        }
    }

    private async void VirtualPcListView_ItemTapped(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        try
        {
            if (selectionChangedEventArgs.CurrentSelection.FirstOrDefault() is VirtualPc selectedVirtualPc)
            {
                await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPc));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    
    private async void AddButton_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new CreateVirtualPc());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void EditConnection_Clicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ImageButton { BindingContext: VirtualPc virtualPc }) return;
            await Navigation.PushAsync(new CreateVirtualPc(virtualPc));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ImageButton button || button.BindingContext is not VirtualPc virtualPc) return;

            bool confirm = await DisplayAlert("Warning!", "Do you really want to delete this virtual pc?", "OK", "Cancel");
            if (!confirm) return;

            virtualPc.RecordState = RecordStates.Deleted;
            foreach (var mapping in Mappings.Where(m => m.VirtualPcId == virtualPc.VirtualPcId && m.RecordState != RecordStates.Deleted))
            {
                mapping.RecordState = RecordStates.Deleted;
            }

            if (virtualPc.OriginalRecordState != RecordStates.Loaded)
            {
                VirtualPCs.Remove(virtualPc);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete virtual pc: {ex.Message}", "OK");
        }
    }
}
