using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVPM.Repositories;

namespace SVPM.Views.LogPages;

public partial class AccountsLogPage
{
    public AccountsLogPage()
    {
        InitializeComponent();
        AccountsLogList.ItemsSource = LogRepository.AccountsLogs.OrderByDescending(al => al.Updated);
    }
}