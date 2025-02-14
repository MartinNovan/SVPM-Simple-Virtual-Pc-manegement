using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SVPM.Pages.MainWindowPages;

namespace SVPM.Pages.ConnectionPages
{
    public partial class SqlConnectionPage
    {
        public SqlConnectionPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void SqlConnectionsListOnLoaded(object? sender, EventArgs eventArgs)
        {
            SqlConnectionListView.ItemsSource = AppShell.SqlConnections;
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

        private async void RefreshButton_Clicked(object? sender, EventArgs e)
        {
            try
            {
                SqlConnectionListView.ItemsSource = AppShell.SqlConnections;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message , "OK");
            }
        }

        private async void EditConnection_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not ImageButton button || button.BindingContext is not Models.SqlConnection connection) return;
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
            {/*
                if (sender is not ImageButton button || button.BindingContext is not Models.SqlConnections connection) return;

                bool confirm = await DisplayAlert("Warning!", "Do you really want to delete a saved connection?", "OK", "Cancel");
                if (!confirm) return;
                
                if (File.Exists(_connectionlist))
                {
                    string json = await File.ReadAllTextAsync(_connectionlist);
                    var connections = JsonSerializer.Deserialize<List<Models.SqlConnections>>(json) ?? new();
                    if (connections.RemoveAll(c => c.Name == connection.Name) > 0)
                    {
                        string updatedJson = JsonSerializer.Serialize(connections, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(_connectionlist, updatedJson);
                        SqlConnections.Remove(connection);
                        await DisplayAlert("Success", "The connection has been successfully deleted.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Connection not found.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "The connections file was not found.", "OK");
                }*/
                return;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete connection: {ex.Message}", "OK");
            }
        }
    }
}
