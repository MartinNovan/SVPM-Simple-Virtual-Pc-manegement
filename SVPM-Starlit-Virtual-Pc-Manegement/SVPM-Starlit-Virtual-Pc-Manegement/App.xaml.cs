using SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new SqlConnectionPage()));
    }
}