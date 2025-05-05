using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVPM.Repositories;
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