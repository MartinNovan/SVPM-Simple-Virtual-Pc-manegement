using SVPM.ViewModels;

namespace SVPM.Views.LogPages;

public partial class AccountsLogPage
{
    public AccountsLogPage()
    {
        InitializeComponent();
        BindingContext = AccountLogViewModel.Instance;
    }
}