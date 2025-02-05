using SVPM_Starlit_Virtual_Pc_Manegement.Pages.CreateRecordsPages;
using SVPM_Starlit_Virtual_Pc_Manegement.Pages.SubWindowPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement.Pages.MainWindowPages;

public partial class VirtualPcPage
{
    public VirtualPcPage()
    {
        InitializeComponent();
    }

    private void VirtualPCsListView_OnLoaded(object? sender, EventArgs e)
    {
        VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Deleted).OrderBy(vpc => vpc.VirtualPcName);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        VirtualPCsListView.ItemsSource = null;
    }
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string? searchText = e.NewTextValue?.ToLower();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Deleted).OrderBy(vpc => vpc.VirtualPcName);
        }
        else
        {
            VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList
                .Where(vpc => vpc is { VirtualPcName: not null } &&
                            (vpc.VirtualPcName.ToLower().Contains(searchText) ||
                             vpc.OwningCustomersNames.ToLower().Contains(searchText)) && vpc.RecordState != Models.RecordStates.Deleted)
                .OrderBy(vpc => vpc.VirtualPcName);
        }
    }

    private async void VirtualPcListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        try
        {
            if (e.Item is Models.VirtualPC selectedVirtualPc)
            {
                await Navigation.PushAsync(new VirtualPcAccountsPage(selectedVirtualPc.VirtualPcID));
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

    private void ReloadButton_OnClicked(object? sender, EventArgs e)
    {
        VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Deleted).OrderBy(vpc => vpc.VirtualPcName);
    }
    private void EditConnection_Clicked(object? sender, EventArgs e)
    {
        Navigation.PushAsync(new CreateVirtualPc());
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ImageButton button || button.BindingContext is not Models.VirtualPC virtualPc) return;

            bool confirm = await DisplayAlert("Warning!", "Do you really want to delete this virtual pc?", "OK", "Cancel");
            if (!confirm) return;

            virtualPc.RecordState = Models.RecordStates.Deleted;
            VirtualPCsListView.ItemsSource = VirtualPcRepository.VirtualPcsList.Where(vpc => vpc.RecordState != Models.RecordStates.Deleted).OrderBy(vpc => vpc.VirtualPcName);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete virtual pc: {ex.Message}", "OK");
        }
    }
}
