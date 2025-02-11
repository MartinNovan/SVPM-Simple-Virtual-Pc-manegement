using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace SVPM;
public static class CustomerRepository
{
    private static readonly SqlConnection Connection = new(GlobalSettings.ConnectionString);
    public static ObservableCollection<Models.Customer> CustomersList { get; set; } = new();
    public static async Task GetAllCustomersAsync()
    {
        CustomersList.Clear();
        await Connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.CustomerTable}";
        var command = new SqlCommand(query, Connection);
        var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            CustomersList.Add(new Models.Customer
            {
                CustomerID = reader.GetGuid(reader.GetOrdinal("CustomerID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                CustomerTag = reader.GetString(reader.GetOrdinal("CustomerTag")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Notes = reader.IsDBNull(reader.GetOrdinal("CustomerNotes")) ? null : reader.GetString(reader.GetOrdinal("CustomerNotes")),
                RecordState = Models.RecordStates.Loaded
            });
        }
        await Connection.CloseAsync();
    }
    public static async Task AddCustomer(Models.Customer customer)
    {
        try
        {
            Connection.Open();

            var query = $@"
            INSERT INTO {GlobalSettings.CustomerTable} (FullName, CustomerTag, Email, Phone, CustomerNotes)
            VALUES (@FullName, @CustomerTag, @Email, @Phone, @Notes)";

            await using var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@FullName", customer.FullName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", customer.Notes ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
            Connection.Close();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding customer: {ex.Message}", "OK");
        }
    }

    public static async Task DeleteCustomer(Guid customerId)
    {
        Connection.Open();
        await using var transaction = Connection.BeginTransaction();
        try
        {
            var query1 = $"DELETE FROM {GlobalSettings.CustomersVirtualPcTable} WHERE CustomerID = @CustomerID";
            await using var command1 = new SqlCommand(query1, Connection, transaction);
            command1.Parameters.AddWithValue("@CustomerID", customerId);
            command1.ExecuteNonQuery();

            var query2 = $"DELETE FROM {GlobalSettings.CustomerTable} WHERE CustomerID = @CustomerID";
            await using var command2 = new SqlCommand(query2, Connection, transaction);
            command2.Parameters.AddWithValue("@CustomerID", customerId);
            command2.ExecuteNonQuery();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting customer: {ex.Message}", "OK");
        }
        finally
        {
            Connection.Close();
        }
    }


    public static async Task UpdateCustomer(Models.Customer customer)
    {
        Connection.Open();
        var transaction = Connection.BeginTransaction();
        try
        {
            var query = $@"
            UPDATE {GlobalSettings.CustomerTable}
            SET FullName = @FullName, CustomerTag = @CustomerTag, Email = @Email, Phone = @Phone, CustomerNotes = @Notes
            WHERE CustomerID = @CustomerID";

            await using var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@FullName", customer.FullName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", customer.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);

            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting customer: {ex.Message}", "OK");
        }
        finally
        {
            Connection.Close();
        }
    }
}
