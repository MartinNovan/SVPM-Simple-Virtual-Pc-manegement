using SVPM.Models;
using SVPM.Services;
using SVPM.ViewModels;
using SVPM.Views.CreatingPages;
using SVPM.Views.SubPages;

namespace SVPM.Views.MainPages;
public partial class VirtualPcPage
{
    public VirtualPcPage()
    {
        InitializeComponent();
        BindingContext = VirtualPcViewModel.Instance;
        VirtualPcViewModel.Instance.FilterVirtualPCs((SearchBar?.Text ?? "").ToLower());
    }
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        VirtualPcViewModel.Instance.FilterVirtualPCs(searchText);
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

    private async void EditButton_Clicked(object? sender, EventArgs e)
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
            await VirtualPcService.Instance.RemoveVirtualPc(virtualPc);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete virtual pc: {ex.Message}", "OK");
        }
    }
}
