using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using static SVPM.Repositories.VirtualPcRepository;

namespace SVPM.Repositories;

public static class AccountRepository
{
    public static ObservableCollection<Account> Accounts { get; } = [];
    public static async Task GetAllAccountsAsync()
    {
        Accounts.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.AccountTable}";
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Accounts.Add(new Account
            {
                AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                AssociatedVirtualPc = new VirtualPc { VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcId")) },
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Password = reader.GetString(reader.GetOrdinal("Password")),
                BackupPassword = reader.IsDBNull(reader.GetOrdinal("BackupPassword")) ? " " : reader.GetString(reader.GetOrdinal("BackupPassword")),
                Admin = reader.GetBoolean(reader.GetOrdinal("Admin")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                RecordState = RecordStates.Loaded
            });
        }
        foreach (var account in Accounts)
        {
            account.AssociatedVirtualPc = VirtualPCs.FirstOrDefault(vpc => vpc.VirtualPcId == account.AssociatedVirtualPc!.VirtualPcId);
            account.InitializeOriginalValues();
        }
    }

    public static async Task AddAccount(Account account)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            var query = $@"
            INSERT INTO {GlobalSettings.AccountTable} (AccountId , VirtualPcId, Username, Password, BackupPassword, Admin, Updated, VerifyHash)
            VALUES (@AccountId, @VirtualPcId, @Username, @Password, @BackupPassword, @Admin, @Updated, @VerifyHash)";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AccountId", account.AccountId);
            if (account.AssociatedVirtualPc != null)
                command.Parameters.AddWithValue("@VirtualPcId", account.AssociatedVirtualPc.VirtualPcId);
            command.Parameters.AddWithValue("@Username", account.Username ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Password", account.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BackupPassword", account.BackupPassword ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Admin", account.Admin);
            command.Parameters.AddWithValue("@Updated", account.Updated);
            command.Parameters.AddWithValue("@VerifyHash", account.VerifyHash);

            await command.ExecuteNonQueryAsync();
            account.RecordState = RecordStates.Loaded;
            account.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding account: {ex.Message}", "OK");
        }
    }

    public static async Task DeleteAccount(Account account)
    {
        if(account.OriginalRecordState != RecordStates.Loaded) {Accounts.Remove(account); return;}
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var isChange = await LookForChange(account, connection, transaction as SqlTransaction);
        if (isChange) return;

        try
        {
            var query = $"DELETE FROM {GlobalSettings.AccountTable} WHERE AccountId = @AccountId";
            await using (var command = new SqlCommand(query, connection, transaction as SqlTransaction))
            {
                command.Parameters.AddWithValue("@AccountId", account.AccountId);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            Accounts.Remove(account);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting account: {ex.Message}", "OK");
        }
    }

    public static async Task UpdateAccount(Account account)
    {
        if (account.OriginalRecordState != RecordStates.Loaded)
        {
            await AddAccount(account);
            return;
        }
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var isChange = await LookForChange(account, connection, transaction as SqlTransaction);
        if (isChange) return;

        try
        {
            var query = $@"
            UPDATE {GlobalSettings.AccountTable}
            SET VirtualPcId = @VirtualPcId, Username = @Username, Password = @Password, BackupPassword = @BackupPassword, Admin = @Admin, Updated = @Updated, VerifyHash = @VerifyHash
            WHERE AccountId = @AccountId";

            await using var command = new SqlCommand(query, connection, transaction as SqlTransaction);
            if (account.AssociatedVirtualPc != null)
                command.Parameters.AddWithValue("@VirtualPcId", account.AssociatedVirtualPc.VirtualPcId);
            command.Parameters.AddWithValue("@Username", account.Username);
            command.Parameters.AddWithValue("@Password", account.Password);
            command.Parameters.AddWithValue("@BackupPassword", account.BackupPassword);
            command.Parameters.AddWithValue("@Admin", account.Admin);
            command.Parameters.AddWithValue("@Updated", account.Updated);
            command.Parameters.AddWithValue("@VerifyHash", account.VerifyHash);
            command.Parameters.AddWithValue("@AccountId", account.AccountId);

            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            account.RecordState = RecordStates.Loaded;
            account.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when updating account: {ex.Message}", "OK");
        }
    }
    private static async Task<bool> LookForChange(Account account, SqlConnection connection, SqlTransaction? transaction = null)
    {
        try
        {
            var selectQuery = $"SELECT VerifyHash FROM {GlobalSettings.AccountTable} WHERE AccountId = @AccountId";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction);
            selectCommand.Parameters.AddWithValue("@AccountId", account.AccountId);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Account not found in the database.", "OK");
                return true;
            }

            await reader.ReadAsync();
            var dbVerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash"));
            reader.Close();

            if (dbVerifyHash != account.OriginalVerifyHash)
            {
                var selectQuery2 = $"Select * FROM {GlobalSettings.AccountTable} WHERE AccountId = @AccountId";
                await using var selectCommand2 = new SqlCommand(selectQuery2, connection, transaction);
                selectCommand2.Parameters.AddWithValue("@AccountId", account.AccountId);
                await using var reader2 = await selectCommand2.ExecuteReaderAsync();

                await reader2.ReadAsync();
                var dbUsername = reader2.GetString(reader2.GetOrdinal("Username"));
                var dbPassword = reader2.GetString(reader2.GetOrdinal("Password"));
                var dbBackupPassword = reader2.GetString(reader2.GetOrdinal("BackupPassword"));
                var dbAdmin = reader2.GetBoolean(reader2.GetOrdinal("Admin"));
                await reader2.CloseAsync();

                bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Conflict Detected",
                    $"Database values have changed.\n\n"
                    + $"Current values in DB:\n"
                    + $"Username: {dbUsername}\nPassword: {dbPassword}\nBackup Password: {dbBackupPassword}\nAdmin: {dbAdmin}\n\n"
                    + $"Your current values:\n"
                    + $"Username: {account.Username}\nPassword: {account.Password}\nBackup Password: {account.BackupPassword}\nAdmin: {account.Admin}\n\n"
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
}
