using SVPM.ViewModels;

namespace SVPM.Views.LogPages;

public partial class MappingsLogPage
{
    public MappingsLogPage()
    {
        InitializeComponent();
        BindingContext = MappingLogViewModel.Instance;
    }
}