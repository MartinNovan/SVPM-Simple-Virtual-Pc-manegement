using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVPM.Repositories;

namespace SVPM.Views.LogPages;

public partial class VirtualPcsLogPage
{
    public VirtualPcsLogPage()
    {
        InitializeComponent();
        VirtualPcsLogList.ItemsSource = LogRepository.VirtualPcsLogs.OrderByDescending(vpcl => vpcl.Updated);
    }
}