using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace SVPM.Repositories;

public static class AccountRepository
{
     public static ObservableCollection<Models.Account> AccountsList { get; set; } = [];
    public static async Task GetAllAccountsAsync()
    {
        AccountsList.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.AccountTable}";
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            AccountsList.Add(new Models.Account
            {
                AccountID = reader.GetGuid(reader.GetOrdinal("AccountID")),
                AssociatedVirtualPc = new Models.VirtualPc { VirtualPcID = reader.GetGuid(reader.GetOrdinal("VirtualPcID")) },
                Username = reader.IsDBNull(reader.GetOrdinal("Username")) ? " " : reader.GetString(reader.GetOrdinal("Username")),
                Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? " " : reader.GetString(reader.GetOrdinal("Password")),
                IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin")),
                LastUpdated = reader.GetDateTime(reader.GetOrdinal("LastUpdated")),
                OriginalPassword = reader.IsDBNull(reader.GetOrdinal("OriginalPassword")) ? " " : reader.GetString(reader.GetOrdinal("OriginalPassword")),
                RecordState = Models.RecordStates.Loaded
            });
        }
        foreach (var account in AccountsList)
        {
            account.AssociatedVirtualPc = VirtualPcRepository.VirtualPcsList.FirstOrDefault(vpc => vpc.VirtualPcID == account.AssociatedVirtualPc!.VirtualPcID);
        }
    }

    public static async Task AddAccount(Models.Account account)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            var query = $@"
            INSERT INTO {GlobalSettings.AccountTable} (AccountID , VirtualPcID, Username, Password, IsAdmin, LastUpdated, OriginalPassword)
            VALUES (@AccountID, @VirtualPcID, @Username, @Password, @IsAdmin, @LastUpdated, @OriginalPassword)";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AccountID", account.AccountID);
            if (account.AssociatedVirtualPc != null)
                command.Parameters.AddWithValue("@VirtualPcID", account.AssociatedVirtualPc.VirtualPcID);
            command.Parameters.AddWithValue("@Username", account.Username ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Password", account.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", account.IsAdmin);
            command.Parameters.AddWithValue("@LastUpdated", account.LastUpdated);
            command.Parameters.AddWithValue("@OriginalPassword", account.OriginalPassword ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding account: {ex.Message}", "OK");
        }
    }

    public static async Task DeleteAccount(Guid accountId)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var query = $"DELETE FROM {GlobalSettings.AccountTable} WHERE AccountID = @AccountID";
            await using (var command = new SqlCommand(query, connection, transaction as SqlTransaction))
            {
                command.Parameters.AddWithValue("@AccountID", accountId);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting account: {ex.Message}", "OK");
        }
    }

    public static async Task UpdateAccount(Models.Account account)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var query = $@"
            UPDATE {GlobalSettings.AccountTable}
            SET VirtualPcID = @VirtualPcID, Username = @Username, Password = @Password, IsAdmin = @IsAdmin, LastUpdated = @LastUpdated, OriginalPassword = @OriginalPassword
            WHERE AccountID = @AccountID";

            await using var command = new SqlCommand(query, connection, transaction as SqlTransaction);
            if (account.AssociatedVirtualPc != null)
                command.Parameters.AddWithValue("@VirtualPcID", account.AssociatedVirtualPc.VirtualPcID);
            command.Parameters.AddWithValue("@Username", account.Username ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Password", account.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", account.IsAdmin);
            command.Parameters.AddWithValue("@LastUpdated", account.LastUpdated);
            command.Parameters.AddWithValue("@OriginalPassword", account.OriginalPassword);
            command.Parameters.AddWithValue("@AccountID", account.AccountID);

            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when updating account: {ex.Message}", "OK");
        }
    }
}
