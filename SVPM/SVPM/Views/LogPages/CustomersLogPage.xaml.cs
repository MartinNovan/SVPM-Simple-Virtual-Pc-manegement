using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVPM.Repositories;

namespace SVPM.Views.LogPages;

public partial class CustomersLogPage
{
    public CustomersLogPage()
    {
        InitializeComponent();
        CustomerLogList.ItemsSource = LogRepository.CustomersLogs.OrderByDescending(cl => cl.Updated);
    }
}