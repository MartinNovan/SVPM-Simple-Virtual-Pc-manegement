using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace SVPM;

public static class CustomerRepository
{
    public static ObservableCollection<Models.Customer> CustomersList { get; set; } = new();

    public static async Task GetAllCustomersAsync()
    {
        CustomersList.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.CustomerTable}";
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

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
    }

    public static async Task AddCustomer(Models.Customer customer)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            var query = $@"
            INSERT INTO {GlobalSettings.CustomerTable} (CustomerID ,FullName, CustomerTag, Email, Phone, CustomerNotes)
            VALUES (@CustomerID, @FullName, @CustomerTag, @Email, @Phone, @Notes)";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
            command.Parameters.AddWithValue("@FullName", customer.FullName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", customer.Notes ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding customer: {ex.Message}", "OK");
        }
    }

    public static async Task DeleteCustomer(Guid customerId)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var deleteQuery = $"DELETE FROM {GlobalSettings.CustomerTable} WHERE CustomerID = @CustomerID";
            await using (var deleteCommand = new SqlCommand(deleteQuery, connection, transaction as SqlTransaction))
            {
                deleteCommand.Parameters.AddWithValue("@CustomerID", customerId);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            var checkQuery = $"SELECT COUNT(*) FROM {GlobalSettings.CustomersVirtualPcTable} WHERE CustomerID = @CustomerID";
            await using (var checkCommand = new SqlCommand(checkQuery, connection, transaction as SqlTransaction))
            {
                checkCommand.Parameters.AddWithValue("@CustomerID", customerId);
                var count = (int)await checkCommand.ExecuteScalarAsync();

                if (count > 0)
                {
                    throw new Exception("Some linked records were not deleted properly.");
                }
            }
/*
            var query = $"DELETE FROM {GlobalSettings.CustomerTable} WHERE CustomerID = @CustomerID";
            await using (var command = new SqlCommand(query, connection, transaction as SqlTransaction))
            {
                command.Parameters.AddWithValue("@CustomerID", customerId);
                await command.ExecuteNonQueryAsync();
            }

            var query1 = $"DELETE FROM {GlobalSettings.CustomersVirtualPcTable} WHERE CustomerID = @CustomerID";
            await using (var command1 = new SqlCommand(query1, connection, transaction as SqlTransaction))
            {
                command1.Parameters.AddWithValue("@CustomerID", customerId);
                await command1.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();*/
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting customer: {ex.Message}", "OK");
        }
    }

    public static async Task UpdateCustomer(Models.Customer customer)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var query = $@"
            UPDATE {GlobalSettings.CustomerTable}
            SET FullName = @FullName, CustomerTag = @CustomerTag, Email = @Email, Phone = @Phone, CustomerNotes = @Notes
            WHERE CustomerID = @CustomerID";

            await using var command = new SqlCommand(query, connection, transaction as SqlTransaction);
            command.Parameters.AddWithValue("@FullName", customer.FullName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", customer.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);

            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when updating customer: {ex.Message}", "OK");
        }
    }
}
