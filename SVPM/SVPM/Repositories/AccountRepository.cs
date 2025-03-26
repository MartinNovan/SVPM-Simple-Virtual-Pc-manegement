using System.Collections.ObjectModel;
using System.Data;
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

        await using var getCommand = new SqlCommand("GetAccounts", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Accounts.Add(new Account
            {
                AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                AssociatedVirtualPc = new VirtualPc { VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcId")) },
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? "" : reader.GetString(reader.GetOrdinal("Password")),
                BackupPassword = reader.IsDBNull(reader.GetOrdinal("BackupPassword")) ? "" : reader.GetString(reader.GetOrdinal("BackupPassword")),
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
            await using var addCommand = new SqlCommand("AddAccount", connection);
            addCommand.CommandType = CommandType.StoredProcedure;
            addCommand.Parameters.AddWithValue("@AccountId", account.AccountId);
            addCommand.Parameters.AddWithValue("@VirtualPcId", account.AssociatedVirtualPc!.VirtualPcId);
            addCommand.Parameters.AddWithValue("@Username", account.Username);
            addCommand.Parameters.AddWithValue("@Password", account.Password ?? String.Empty);
            addCommand.Parameters.AddWithValue("@BackupPassword", account.BackupPassword ?? String.Empty);
            addCommand.Parameters.AddWithValue("@Admin", account.Admin);
            addCommand.Parameters.AddWithValue("@Updated", account.Updated);
            addCommand.Parameters.AddWithValue("@VerifyHash", account.VerifyHash);

            await addCommand.ExecuteNonQueryAsync();
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
            await using var deleteCommand = new SqlCommand("DeleteAccount", connection, transaction as SqlTransaction);
            deleteCommand.CommandType = CommandType.StoredProcedure;
            deleteCommand.Parameters.AddWithValue("@AccountId", account.AccountId);

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
            await using var updateCommand = new SqlCommand("UpdateAccount", connection, transaction as SqlTransaction);
            updateCommand.CommandType = CommandType.StoredProcedure;
            updateCommand.Parameters.AddWithValue("@VirtualPcId", account.AssociatedVirtualPc!.VirtualPcId);
            updateCommand.Parameters.AddWithValue("@Username", account.Username);
            updateCommand.Parameters.AddWithValue("@Password", account.Password ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@BackupPassword", account.BackupPassword ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Admin", account.Admin);
            updateCommand.Parameters.AddWithValue("@Updated", account.Updated);
            updateCommand.Parameters.AddWithValue("@VerifyHash", account.VerifyHash);
            updateCommand.Parameters.AddWithValue("@AccountId", account.AccountId);

            await updateCommand.ExecuteNonQueryAsync();
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
            await using var checkCommand = new SqlCommand("CheckForAccountConflict", connection, transaction);
            checkCommand.CommandType = CommandType.StoredProcedure;
            checkCommand.Parameters.AddWithValue("@AccountId", account.AccountId);
            checkCommand.Parameters.AddWithValue("@OriginalVerifyHash", account.OriginalVerifyHash);

            await using var reader = await checkCommand.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync();
                var dbUsername = reader.GetString(reader.GetOrdinal("Username"));
                var dbPassword = reader.GetString(reader.GetOrdinal("Password"));
                var dbBackupPassword = reader.GetString(reader.GetOrdinal("BackupPassword"));
                var dbAdmin = reader.GetBoolean(reader.GetOrdinal("Admin"));
                await reader.CloseAsync();

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
