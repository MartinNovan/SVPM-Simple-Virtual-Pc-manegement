using Microsoft.Data.SqlClient;

namespace SVPM;

public static class CustomersVirtualPCsRepository
{
    public static List<Models.Mapping> MappingList { get; set; } = new();

    public static async Task GetAllMappingAsync()
    {
        var mappings = new List<Models.Mapping>();
        await using (var connection = new SqlConnection(GlobalSettings.ConnectionString))
        {
            await connection.OpenAsync();

            var query = $"SELECT * FROM {GlobalSettings.CustomersVirtualPcTable}";
            await using (var command = new SqlCommand(query, connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    mappings.Add(new Models.Mapping
                    {
                        MappingID = reader.GetGuid(0),
                        CustomerID = reader.GetGuid(1),
                        VirtualPcID = reader.GetGuid(2),
                        RecordState = Models.RecordStates.Loaded,
                        inDatabase = true
                    });
                }
            }
        }
        MappingList = mappings;
    }

    public static async Task AddMappingAsync(Models.Mapping mapping)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        var query = $@"
                INSERT INTO {GlobalSettings.CustomersVirtualPcTable} (CustomerID, VirtualPcID)
                VALUES (@CustomerID, @VirtualPcID)";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerID", mapping.CustomerID);
        command.Parameters.AddWithValue("@VirtualPcID", mapping.VirtualPcID);
        await command.ExecuteNonQueryAsync();
        mapping.inDatabase = true;
    }

    public static async Task DeleteMappingAsync(Models.Mapping mapping)
    {
        if (!mapping.inDatabase) return;
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var query = $@"
                    DELETE FROM {GlobalSettings.CustomersVirtualPcTable}
                    WHERE CustomerID = @CustomerID AND VirtualPcID = @VirtualPcID";

            await using (var command = new SqlCommand(query, connection, transaction as SqlTransaction))
            {
                command.Parameters.AddWithValue("@CustomerID", mapping.CustomerID);
                command.Parameters.AddWithValue("@VirtualPcID", mapping.VirtualPcID);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
