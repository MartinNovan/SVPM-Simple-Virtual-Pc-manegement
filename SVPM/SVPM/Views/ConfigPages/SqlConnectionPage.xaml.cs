using SVPM.Models;
using SVPM.ViewModels;
using SVPM.Views.CreatingPages;

namespace SVPM.Views.ConfigPages
{
    public partial class SqlConnectionPage
    {
        public SqlConnectionPage()
        {
            InitializeComponent();
            BindingContext = SqlConnectionViewModel.Instance;
        }

        private async void CreateConnection_Clicked(object? sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new SqlCreateConnectionPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void EditConnection_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not SqlConnection connection) return;
                await Navigation.PushAsync(new SqlCreateConnectionPage(connection));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not SqlConnection connection) return;

                bool confirm = await DisplayAlert("Warning!", "Do you really want to delete a saved connection?", "OK", "Cancel");
                if (!confirm) return;
                
                await SqlConnectionViewModel.Instance.DeleteConnectionAsync(connection);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete connection: {ex.Message}", "OK");
            }
        }
    }
}
