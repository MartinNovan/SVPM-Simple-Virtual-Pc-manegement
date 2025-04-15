namespace SVPM.Views.LogPages;

public partial class CustomersLogPage
{
    public CustomersLogPage()
    {
        InitializeComponent();
    }

    private void CustomerLogList_OnLoaded(object? sender, EventArgs e)
    {
        CustomerLogList.ItemsSource = ViewModels.CustomerLogViewModel.CustomersLogs.OrderByDescending(cl => cl.Updated);
    }
}