using SVPM.ViewModels;
using SqlConnection = SVPM.Models.SqlConnection;

namespace SVPM.Views.CreatingPages
{
    public partial class SqlCreateConnectionPage
    {
        public SqlCreateConnectionPage(SqlConnection? connection = null)
        {
            InitializeComponent();
            if (connection != null)
            {
                PopulateFields(connection);
            }
            else
            {
                WindowsAuthSwitch.IsToggled = true;
                CertificateSwitch.IsToggled = false;
            }
        }

        private void PopulateFields(SqlConnection? connection)
        {
            if (connection == null) return;
            NameEntry.Text = connection.Name ?? string.Empty;
            ServerEntry.Text = connection.ServerAddress ?? string.Empty;
            DatabaseEntry.Text = connection.DatabaseName ?? string.Empty;

            WindowsAuthSwitch.IsToggled = connection.UseWindowsAuth;
            SetWinAuthVisibility(!connection.UseWindowsAuth);
            UsernameEntry.Text = connection.Username ?? string.Empty;
            PasswordEntry.Text = connection.Password ?? string.Empty;

            CertificateSwitch.IsToggled = connection.UseCertificate;
        }

        private void SetWinAuthVisibility(bool isVisible)
        {
            UsernameText.IsVisible = isVisible;
            UsernameEntry.IsVisible = isVisible;
            PasswordText.IsVisible = isVisible;
            PasswordEntry.IsVisible = isVisible;
        }

        private async void OnConfirmButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
                    string.IsNullOrWhiteSpace(ServerEntry.Text) ||
                    string.IsNullOrWhiteSpace(DatabaseEntry.Text))
                {
                    await DisplayAlert("Info","Please fill in all required fields.", "OK");
                    return;
                }
                await SqlConnectionViewModel.Instance.SaveConnectionAsync(GenerateConnection());
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",$"Connection failed: {ex.Message}", "OK");
            }
        }

        private SqlConnection GenerateConnection()
        {
            return new SqlConnection
            {
                Name = NameEntry.Text,
                ServerAddress = ServerEntry.Text,
                DatabaseName = DatabaseEntry.Text,
                UseWindowsAuth = WindowsAuthSwitch.IsToggled,
                Username = UsernameEntry.Text,
                Password = PasswordEntry.Text,
                UseCertificate = CertificateSwitch.IsToggled,
                CertificatePath = CertificatePathEntry.Text
            };
        }

        private void OnWindowsAuthToggled(object sender, ToggledEventArgs e)
        {
            SetWinAuthVisibility(!e.Value);
        }

        private void CertificateToggled(object sender, ToggledEventArgs e)
        {
            CertificatePathEntry.IsVisible = e.Value;
            CertificateText.IsVisible = e.Value;
        }
    }
}