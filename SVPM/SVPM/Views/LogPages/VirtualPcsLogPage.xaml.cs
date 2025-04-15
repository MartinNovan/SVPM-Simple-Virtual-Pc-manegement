using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVPM.Repositories;
using SVPM.ViewModels;

namespace SVPM.Views.LogPages;

public partial class VirtualPcsLogPage
{
    public VirtualPcsLogPage()
    {
        InitializeComponent();
    }

    private void VirtualPcsLogList_OnLoaded(object? sender, EventArgs e)
    {
        VirtualPcsLogList.ItemsSource = VirtualPcLogViewModel.VirtualPcsLogs.OrderByDescending(vpcl => vpcl.Updated);
    }
}