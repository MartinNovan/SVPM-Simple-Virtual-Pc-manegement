using SVPM.ViewModels;

namespace SVPM;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }
    protected override async void OnStart()
    {
        try
        {
            base.OnStart();
            await SqlConnectionViewModel.Instance.LoadConnectionsAsync();
        }
        catch (Exception ex)
        {
            Current?.Windows[0].Page?.DisplayAlert("Error", $"Failed to load connections: {ex.Message}", "OK");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}