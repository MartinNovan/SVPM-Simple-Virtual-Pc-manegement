using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;

public static class CustomerRepository
{
    public static ObservableCollection<Customer> Customers { get; } = [];
    public static async Task GetCustomersAsync()
    {
        Customers.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        await using var getCommand = new SqlCommand("GetCustomers", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var customer = new Customer
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
            };
            customer.InitializeOriginalValues();
            Customers.Add(customer);
        }
    }

    public static async Task AddCustomer(Customer customer)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            await using var addCommand = new SqlCommand("AddCustomer", connection);
            addCommand.CommandType = CommandType.StoredProcedure;
            addCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
            addCommand.Parameters.AddWithValue("@FullName", customer.FullName);
            addCommand.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag);
            addCommand.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@Notes", customer.Notes ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@VerifyHash", customer.VerifyHash);

            await addCommand.ExecuteNonQueryAsync();

            customer.RecordState = RecordStates.Loaded;
            customer.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding customer: {ex.Message} \n Name: {customer.FullName} \n Tag: {customer.CustomerTag} \n Email: {customer.Email} \n Phone: {customer.Phone} \n Notes: {customer.Notes}", "OK");
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

            await using var deleteCommand = new SqlCommand("DeleteCustomer", connection, transaction as SqlTransaction);
            deleteCommand.CommandType = CommandType.StoredProcedure;
            deleteCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
            await deleteCommand.ExecuteNonQueryAsync();

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

            await using var updateCommand = new SqlCommand("UpdateCustomer", connection, transaction as SqlTransaction); // Přidáno propojení s transakcí
            updateCommand.CommandType = CommandType.StoredProcedure;
            updateCommand.Parameters.AddWithValue("@FullName", customer.FullName);
            updateCommand.Parameters.AddWithValue("@CustomerTag", customer.CustomerTag);
            updateCommand.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Notes", customer.Notes ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@VerifyHash", customer.VerifyHash);
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
            await using var checkCommand = new SqlCommand("CheckForCustomerConflict", connection, transaction);
            checkCommand.CommandType = CommandType.StoredProcedure;
            checkCommand.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
            checkCommand.Parameters.AddWithValue("@OriginalVerifyHash", customer.OriginalVerifyHash);

            await using var reader = await checkCommand.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync();

                var dbFullName = reader.GetString(reader.GetOrdinal("FullName"));
                var dbCustomerTag = reader.GetString(reader.GetOrdinal("CustomerTag"));
                var dbEmail = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email"));
                var dbPhone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone"));
                var dbNotes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes"));

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

                return !confirm;
            }

            await reader.CloseAsync();
            return false;
        }
        catch (SqlException ex) when (ex.Message.Contains("Customer not found"))
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Customer not found in the database.", "OK");
            return true;
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when looking for change: {ex.Message}", "OK");
            return true;
        }
    }
}
