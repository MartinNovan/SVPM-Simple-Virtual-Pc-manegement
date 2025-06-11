using System.Data;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using SVPM.ViewModels;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;

public static class AccountRepository
{
    public static async Task<List<Account>> GetAllAccountsAsync()
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        await using var getCommand = new SqlCommand("GetAccounts", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();
        
        var accounts = new List<Account>();
        while (await reader.ReadAsync())
        {
            var account = new Account
            {
                AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? "" : reader.GetString(reader.GetOrdinal("Password")),
                BackupPassword = reader.IsDBNull(reader.GetOrdinal("BackupPassword")) ? "" : reader.GetString(reader.GetOrdinal("BackupPassword")),
                Admin = reader.GetBoolean(reader.GetOrdinal("Admin")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                RecordState = RecordStates.Loaded
            };
            account.InitializeOriginalValues();
            accounts.Add(account);
        }
        return accounts;
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
            addCommand.Parameters.AddWithValue("@VirtualPcId", account.VirtualPcId);
            addCommand.Parameters.AddWithValue("@Username", account.Username);
            addCommand.Parameters.AddWithValue("@Password", account.Password ?? String.Empty);
            addCommand.Parameters.AddWithValue("@BackupPassword", account.BackupPassword ?? String.Empty);
            addCommand.Parameters.AddWithValue("@Admin", account.Admin);
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
        if(account.OriginalRecordState != RecordStates.Loaded) {await AccountViewModel.Instance.RemoveAccount(account); return;}
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var isChange = await LookForChange(account, connection);
        if (isChange) return;

        try
        {
            await using var deleteCommand = new SqlCommand("DeleteAccount", connection);
            deleteCommand.CommandType = CommandType.StoredProcedure;
            deleteCommand.Parameters.AddWithValue("@AccountId", account.AccountId);

            await deleteCommand.ExecuteNonQueryAsync();
            await AccountViewModel.Instance.RemoveAccount(account);
        }
        catch (Exception ex)
        {
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

        var isChange = await LookForChange(account, connection);
        if (isChange) return;

        try
        {
            await using var updateCommand = new SqlCommand("UpdateAccount", connection);
            updateCommand.CommandType = CommandType.StoredProcedure;
            updateCommand.Parameters.AddWithValue("@VirtualPcId", account.VirtualPcId);
            updateCommand.Parameters.AddWithValue("@Username", account.Username);
            updateCommand.Parameters.AddWithValue("@Password", account.Password ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@BackupPassword", account.BackupPassword ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Admin", account.Admin);
            updateCommand.Parameters.AddWithValue("@VerifyHash", account.VerifyHash);
            updateCommand.Parameters.AddWithValue("@AccountId", account.AccountId);

            await updateCommand.ExecuteNonQueryAsync();
            account.RecordState = RecordStates.Loaded;
            account.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when updating account: {ex.Message}", "OK");
        }
    }
    private static async Task<bool> LookForChange(Account account, SqlConnection connection)
    {
        try
        {
            await using var checkCommand = new SqlCommand("CheckForAccountConflict", connection);
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
