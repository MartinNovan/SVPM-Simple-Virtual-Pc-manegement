using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVPM.Repositories;

namespace SVPM.Views.LogPages;

public partial class MappingsLogPage
{
    public MappingsLogPage()
    {
        InitializeComponent();
        MappingsLogList.ItemsSource = LogRepository.MappingsLogs.OrderByDescending(ml => ml.Updated);
    }
}