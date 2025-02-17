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
                CustomerID = reader.GetGuid(reader.GetOrdinal("CustomerID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                CustomerTag = reader.GetString(reader.GetOrdinal("CustomerTag")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Notes = reader.IsDBNull(reader.GetOrdinal("CustomerNotes")) ? null : reader.GetString(reader.GetOrdinal("CustomerNotes")),
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
        if(customer.OriginalRecordState != RecordStates.Loaded) return;
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var isChange = await LookForChange(customer, connection, transaction as SqlTransaction);
            if (isChange) return;

            var deleteQuery = $"DELETE FROM {GlobalSettings.CustomerTable} WHERE CustomerID = @CustomerID";
            await using (var deleteCommand = new SqlCommand(deleteQuery, connection, transaction as SqlTransaction))
            {
                deleteCommand.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            var checkQuery = $"SELECT COUNT(*) FROM {GlobalSettings.CustomersVirtualPcTable} WHERE CustomerID = @CustomerID";
            await using (var checkCommand = new SqlCommand(checkQuery, connection, transaction as SqlTransaction))
            {
                checkCommand.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                var count = (int)(await checkCommand.ExecuteScalarAsync() ?? 0);

                if (count > 0)
                {
                    throw new Exception("Some linked records were not deleted properly.");
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

            var updateQuery = $@"
            UPDATE {GlobalSettings.CustomerTable}
            SET FullName = @FullName, CustomerTag = @CustomerTag, Email = @Email, Phone = @Phone, CustomerNotes = @Notes
            WHERE CustomerID = @CustomerID";

            await using var updateCommand = new SqlCommand(updateQuery, connection, transaction as SqlTransaction);
            updateCommand.Parameters.AddWithValue("@FullName", customer.FullName ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Notes", customer.Notes ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@CustomerID", customer.CustomerID);

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
            var selectQuery = $@"
            SELECT FullName, CustomerTag, Email, Phone, CustomerNotes
            FROM {GlobalSettings.CustomerTable}
            WHERE CustomerID = @CustomerID";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction);
            selectCommand.Parameters.AddWithValue("@CustomerID", customer.CustomerID);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Customer not found in the database.", "OK");
                return true;
            }

            await reader.ReadAsync();
            var dbFullName = reader["FullName"] as string;
            var dbCustomerTag = reader["CustomerTag"] as string;
            var dbEmail = reader["Email"] as string;
            var dbPhone = reader["Phone"] as string;
            var dbNotes = reader["CustomerNotes"] as string;
            reader.Close();

            if ((dbFullName != customer.OriginalFullName) || (dbCustomerTag != customer.OriginalCustomerTag) ||
                (dbEmail != customer.OriginalEmail) || (dbPhone != customer.OriginalPhone) ||
                (dbNotes != customer.OriginalNotes))
            {
                bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Conflict Detected",
                    $"Database values have changed.\n\n"
                    + $"Current values in DB:\n"
                    + $"Full Name: {dbFullName}\nCustomer Tag: {dbCustomerTag}\nEmail: {dbEmail}\nPhone: {dbPhone}\nNotes: {dbNotes}\n\n"
                    + $"Values you originally loaded:\n"
                    + $"Full Name: {customer.OriginalFullName}\nCustomer Tag: {customer.OriginalCustomerTag}\nEmail: {customer.OriginalEmail}\nPhone: {customer.OriginalPhone}\nNotes: {customer.OriginalNotes}\n\n"
                    + $"Your new values to save:\n"
                    + $"Full Name: {customer.FullName}\nCustomer Tag: {customer.CustomerTag}\nEmail: {customer.Email}\nPhone: {customer.Phone}\nNotes: {customer.Notes}\n\n"
                    + "Do you want to proceed?",
                    "Yes", "No");

                if (!confirm)
                {
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when looking for change:{ex.Message}","OK");
            return true;
        }
        return true;
    }

private static async Task<bool> LookForConflict(Customer customer, SqlConnection connection)
{
    try
        {
            var selectQuery = $"SELECT * FROM {GlobalSettings.CustomerTable} WHERE CustomerTag = @CustomerTag";

            await using var selectCommand = new SqlCommand(selectQuery, connection);
            selectCommand.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (reader.HasRows)
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
