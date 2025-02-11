using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace SVPM;

public class AccountRepository
{
    private static readonly SqlConnection Connection = new(GlobalSettings.ConnectionString);
    public static ObservableCollection<Models.Account> AccountsList { get; set; } = new();
    public static async Task GetAllAccountsAsync()
    {
        AccountsList.Clear();
        await Connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.AccountTable}";
        var command = new SqlCommand(query, Connection);
        var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            AccountsList.Add(new Models.Account
            {
                AccountID = reader.GetGuid(0),
                AssociatedVirtualPc = new Models.VirtualPC { VirtualPcID = reader.GetGuid(1) },
                Username = reader.IsDBNull(2) ? " " : reader.GetString(2),
                Password = reader.IsDBNull(3) ? " " : reader.GetString(3),
                IsAdmin = reader.GetBoolean(4),
                LastUpdated = reader.GetDateTime(5),
                OriginalPassword = reader.IsDBNull(6) ? " " : reader.GetString(6),
                RecordState = Models.RecordStates.Loaded,
            });
        }
        await Connection.CloseAsync();
        foreach (var account in AccountsList)
        {
            account.AssociatedVirtualPc = VirtualPcRepository.VirtualPcsList
                .FirstOrDefault(vpc => vpc.VirtualPcID == account.AssociatedVirtualPc!.VirtualPcID);
        }
    }

    public static async Task AddAccount(Models.Account account)
    {
        Connection.Open();
        try
        {
            var query = $@"
            INSERT INTO {GlobalSettings.AccountTable} ( VirtualPcID, Username, Password, IsAdmin, OriginalPassword)
            VALUES ( @VirtualPcID, @Username, @Password, @IsAdmin, @OriginalPassword)";

            using var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@VirtualPcID", account.AssociatedVirtualPc!.VirtualPcID);
            command.Parameters.AddWithValue("@Username", account.Username ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Password", account.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", account.IsAdmin);
            command.Parameters.AddWithValue("@OriginalPassword", account.OriginalPassword);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public static async Task DeleteAccount(Guid accountId)
    {
        Connection.Open();
        var transaction = Connection.BeginTransaction();
        try
        {
            var query = $"DELETE FROM {GlobalSettings.AccountTable} WHERE AccountID = @AccountID";
            using var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@AccountID", accountId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public static async Task UpdateAccount(Models.Account account)
    {
        Connection.Open();
        var transaction = Connection.BeginTransaction();
        try
        {
            var query = $@"
            UPDATE {GlobalSettings.AccountTable}
            SET VirtualPcID = @VirtualPcID, Username = @Username, Password = @Password, IsAdmin = @IsAdmin, OriginalPassword = @OriginalPassword
            WHERE AccountID = @AccountID";

            using var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@VirtualPcID", account.AssociatedVirtualPc!.VirtualPcID);
            command.Parameters.AddWithValue("@Username", account.Username ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Password", account.Password ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsAdmin", account.IsAdmin);
            command.Parameters.AddWithValue("@OriginalPassword", account.OriginalPassword);
            command.Parameters.AddWithValue("@AccountID", account.AccountID);

            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            Connection.Close();
        }
    }
}
