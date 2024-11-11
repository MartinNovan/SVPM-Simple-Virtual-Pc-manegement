using SVPM_Starlit_Virtual_Pc_Manegement.Pages.ConnectionPages;

namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new SqlConnectionPage());
        }
    }
}
    