using SVPM.Pages.ConnectionPages;

namespace SVPM.Pages.MainWindowPages
{
    public partial class MainTabbedPage
    {
        public MainTabbedPage()
        {
            InitializeComponent();
        }
        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async void () =>
            {
                try
                {
                    bool answer = await DisplayAlert("Warning","Do you want to exit? Any unsaved changes will be lost.","Yes", "No");

                    if (answer)
                    {
                        GlobalSettings.ConnectionString = null;
                        CustomerRepository.CustomersList.Clear();
                        CustomersVirtualPCsRepository.MappingList.Clear();
                        VirtualPcRepository.VirtualPcsList.Clear();
                        AccountRepository.AccountsList.Clear();
                        Application.Current!.Windows[0].Page = new NavigationPage(new SqlConnectionPage());
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
                }
            });
            return true;
        }

        private async void PullFromDatabase(object? sender, EventArgs e)
        {
            try
            {
                bool answer = await DisplayAlert("Warning", "This will rewrite locally stored data from database. Any un-pushed changes will be deleted.", "Continue", "Cancel");
                if (!answer)
                {
                    return;
                }
                await Navigation.PushAsync(new LoadingPage(false, false));
            }
            catch (Exception ex)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void PushToDatabase(object? sender, EventArgs e)
        {
            try
            {
                bool answer = await DisplayAlert("Warning", "This will push all local changes to the database. Any changes made on the database will be overwritten and cannot be changed back!", "Continue", "Cancel");
                if (!answer)
                {
                    return;
                }
                await Navigation.PushAsync(new LoadingPage(true, false));
            }
            catch (Exception ex)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
