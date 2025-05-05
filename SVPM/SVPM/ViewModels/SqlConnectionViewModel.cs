using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using SVPM.Repositories;
using SqlConnection = SVPM.Models.SqlConnection;

namespace SVPM.ViewModels;

public class SqlConnectionViewModel
{
    public static SqlConnectionViewModel Instance { get; } = new();
    private ObservableCollection<SqlConnection> SqlConnections { get; } = new();
    public ObservableCollection<SqlConnection> SortedSqlConnections { get; } = new();
    
    private SqlConnectionViewModel()
    {
        SqlConnections.CollectionChanged += (_, _) => SortConnections();
    }
    
    private void SortConnections()
    {
        SortedSqlConnections.Clear();
        foreach (var connection in SqlConnections.OrderBy(connection => connection.Name))
        {
            SortedSqlConnections.Add(connection);
        }
    }
    
    public async Task LoadConnectionsAsync()
    {
        var connections = await SqlConnectionRepository.GetSqlConnections();
        
        SqlConnections.Clear();
        foreach (var connection in connections)
        {
            SqlConnections.Add(connection);
        }
    }
    
    public async Task SelectConnectionAsync(SqlConnection connection)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = connection.ServerAddress,
                InitialCatalog = connection.DatabaseName,
                IntegratedSecurity = connection.UseWindowsAuth,
                TrustServerCertificate = !connection.UseCertificate
            };

            if (!connection.UseWindowsAuth)
            {
                builder.UserID = connection.Username;
                builder.Password = connection.Password;
            }

            if (connection.UseCertificate && !string.IsNullOrWhiteSpace(connection.CertificatePath))
            {
                builder.ServerCertificate = connection.CertificatePath;
            }

            GlobalSettings.ConnectionString = builder.ToString();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
        }
    }
    
    public async Task SaveConnectionAsync(SqlConnection connection)
    {
        try
        {
            var matchingConnection = SqlConnections.FirstOrDefault(c => c.Name == connection.Name);
            if (matchingConnection != null)
            {
                bool asnwer = await Application.Current!.Windows[0].Page!.DisplayAlert("Info", "Existing connection, with this name, will be updated.", "OK", "Cancel");
                if(!asnwer) return;
                SqlConnections.Remove(matchingConnection);
            }
            else
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Info", "New connection has been added.", "OK");
            }
            SqlConnections.Add(connection);
            await SqlConnectionRepository.SaveConnectionsToFileAsync(SqlConnections.ToList());

        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Failed to save connection: {ex.Message}", "OK");
        }
    }
    
    public async Task DeleteConnectionAsync(SqlConnection connection)
    {
        try
        {
            SqlConnections.Remove(connection);
            await SqlConnectionRepository.SaveConnectionsToFileAsync(SqlConnections.ToList());
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Failed to delete connection: {ex.Message}", "OK");
        }
    }
}