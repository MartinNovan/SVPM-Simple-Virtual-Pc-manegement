using SVPM.ViewModels;

namespace SVPM.Views.LogPages;

public partial class CustomersLogPage
{
    public CustomersLogPage()
    {
        InitializeComponent();
        BindingContext = CustomerLogViewModel.Instance;
    }
}