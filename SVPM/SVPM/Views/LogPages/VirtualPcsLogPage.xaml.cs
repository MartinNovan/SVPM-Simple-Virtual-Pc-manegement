using SVPM.ViewModels;

namespace SVPM.Views.LogPages;

public partial class VirtualPcsLogPage
{
    public VirtualPcsLogPage()
    {
        InitializeComponent();
        BindingContext = VirtualPcLogViewModel.Instance;
    }
}