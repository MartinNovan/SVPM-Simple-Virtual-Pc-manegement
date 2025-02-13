using SVPM.Pages.ConnectionPages;
using SVPM.Pages.MainWindowPages;

namespace SVPM;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        //return new Window(new NavigationPage(new SqlConnectionPage()));
        return new Window(new AppShell());
    }
}