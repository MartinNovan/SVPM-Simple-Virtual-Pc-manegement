namespace SVPM_Starlit_Virtual_Pc_Manegement
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new SqlConnectionPage()); // správně obaleno
        }
    }
}
