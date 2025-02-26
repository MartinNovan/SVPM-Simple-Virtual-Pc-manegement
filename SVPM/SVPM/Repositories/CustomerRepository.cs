using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;

public static class CustomerRepository
{
    public static ObservableCollection<Customer> Customers { get; } = [];
    public static async Task GetAllCustomersAsync()
    {
        Customers.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.CustomerTable}";
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Customers.Add(new Customer
            {
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                CustomerTag = reader.GetString(reader.GetOrdinal("CustomerTag")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                RecordState = RecordStates.Loaded,
            });
        }
        foreach (var customer in Customers)
        {
            customer.InitializeOriginalValues();
        }
    }

    public static async Task AddCustomer(Customer customer)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            var conflict = await LookForConflict(customer, connection);
            if (conflict) return;
            var query = $@"
            INSERT INTO {GlobalSettings.CustomerTable} (CustomerId ,FullName, CustomerTag, Email, Phone, Notes, VerifyHash, Updated)
            VALUES (@CustomerId, @FullName, @CustomerTag, @Email, @Phone, @Notes, @VerifyHash, @Updated)";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
            command.Parameters.AddWithValue("@FullName", customer.FullName ?? String.Empty);
            command.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag ?? String.Empty);
            command.Parameters.AddWithValue("@Email", customer.Email ?? String.Empty);
            command.Parameters.AddWithValue("@Phone", customer.Phone ?? String.Empty);
            command.Parameters.AddWithValue("@Notes", customer.Notes ?? String.Empty);
            command.Parameters.AddWithValue("@VerifyHash", customer.VerifyHash);
            command.Parameters.AddWithValue("@Updated", customer.Updated);

            await command.ExecuteNonQueryAsync();

            customer.RecordState = RecordStates.Loaded;
            customer.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding customer: {ex.Message}", "OK");
        }
    }

    public static async Task DeleteCustomer(Customer customer)
    {
        if (customer.OriginalRecordState != RecordStates.Loaded){ Customers.Remove(customer); return;}
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var isChange = await LookForChange(customer, connection, transaction as SqlTransaction);
            if (isChange) return;

            var deleteQuery = $"DELETE FROM {GlobalSettings.CustomerTable} WHERE CustomerId = @CustomerId";
            await using (var deleteCommand = new SqlCommand(deleteQuery, connection, transaction as SqlTransaction))
            {
                deleteCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            var checkQuery = $"SELECT COUNT(*) FROM {GlobalSettings.CustomersVirtualPcTable} WHERE CustomerId = @CustomerId";
            await using (var checkCommand = new SqlCommand(checkQuery, connection, transaction as SqlTransaction))
            {
                checkCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                var count = (int)(await checkCommand.ExecuteScalarAsync() ?? 0);

                if (count > 0)
                {
                    await Application.Current?.Windows[0].Page?.DisplayAlert("Error", "Some mappings weren't properly deleted!", "OK")!;
                }
            }
            await transaction.CommitAsync();
            Customers.Remove(customer);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting customer: {ex.Message}", "OK");
        }
    }

    public static async Task UpdateCustomer(Customer customer)
    {
        if (customer.OriginalRecordState != RecordStates.Loaded)
        {
            await AddCustomer(customer);
            return;
        }
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var isChange = await LookForChange(customer, connection, transaction as SqlTransaction);
            if (isChange) return;

            var conflict = await LookForConflict(customer, connection);
            if (conflict) return;

            var updateQuery = $@"UPDATE {GlobalSettings.CustomerTable}
            SET FullName = @FullName, CustomerTag = @CustomerTag, Email = @Email, Phone = @Phone, Notes = @Notes, VerifyHash = @VerifyHash, Updated = @Updated
            WHERE CustomerId = @CustomerId";

            await using var updateCommand = new SqlCommand(updateQuery, connection, transaction as SqlTransaction);
            updateCommand.Parameters.AddWithValue("@FullName", customer.FullName);
            updateCommand.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag);
            updateCommand.Parameters.AddWithValue("@Email", customer.Email ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Phone", customer.Phone ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Notes", customer.Notes ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@VerifyHash", customer.VerifyHash);
            updateCommand.Parameters.AddWithValue("@Updated", customer.Updated);
            updateCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);

            await updateCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            customer.RecordState = RecordStates.Loaded;
            customer.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when updating customer: {ex.Message}", "OK");
        }
    }

    private static async Task<bool> LookForChange(Customer customer, SqlConnection connection, SqlTransaction? transaction = null)
    {
        try
        {
            var selectQuery = $"SELECT VerifyHash FROM {GlobalSettings.CustomerTable} WHERE CustomerId = @CustomerId";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction);
            selectCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Customer not found in the database.", "OK");
                return true;
            }

            await reader.ReadAsync();
            var dbVerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash"));
            reader.Close();

            if (dbVerifyHash != customer.OriginalVerifyHash)
            {
                var selectQuery2 = $"Select * FROM {GlobalSettings.CustomerTable} WHERE CustomerId = @CustomerId";
                await using var selectCommand2 = new SqlCommand(selectQuery2, connection, transaction);
                selectCommand2.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                await using var reader2 = await selectCommand2.ExecuteReaderAsync();

                await reader.ReadAsync();
                var dbFullName = reader2.GetString(reader2.GetOrdinal("FullName"));
                var dbCustomerTag = reader2.GetString(reader2.GetOrdinal("CustomerTag"));
                var dbEmail = reader2.IsDBNull(reader2.GetOrdinal("Email")) ? null : reader2.GetString(reader2.GetOrdinal("Email"));
                var dbPhone = reader2.IsDBNull(reader2.GetOrdinal("Phone")) ? null : reader2.GetString(reader2.GetOrdinal("Phone"));
                var dbNotes = reader2.IsDBNull(reader2.GetOrdinal("CustomerNotes")) ? null : reader2.GetString(reader2.GetOrdinal("CustomerNotes"));
                await reader.CloseAsync();

                bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Conflict Detected",
                    $"Database values have changed.\n\n"
                    + $"Current values in DB:\n"
                    + $"Full Name: {dbFullName}\nCustomer Tag: {dbCustomerTag}\nEmail: {dbEmail}\nPhone: {dbPhone}\nNotes: {dbNotes}\n\n"
                    + $"Your current values:\n"
                    + $"Full Name: {customer.FullName}\nCustomer Tag: {customer.CustomerTag}\nEmail: {customer.Email}\nPhone: {customer.Phone}\nNotes: {customer.Notes}\n\n"
                    + "Do you want to proceed?",
                    "Yes", "No");

                if (!confirm)
                {
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when looking for change:{ex.Message}","OK");
            return true;
        }
    }

    private static async Task<bool> LookForConflict(Customer customer, SqlConnection connection, SqlTransaction? transaction = null)
    {
        try
        {
            var selectQuery = $"SELECT * FROM {GlobalSettings.CustomerTable} WHERE CustomerTag = @CustomerTag";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction);
            selectCommand.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (reader.HasRows && customer.CustomerTag != customer.OriginalCustomerTag)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Customer with this tag was already added to database.", "OK");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when looking for conflict:{ex.Message}","OK");
            return true;
        }
    }
}
