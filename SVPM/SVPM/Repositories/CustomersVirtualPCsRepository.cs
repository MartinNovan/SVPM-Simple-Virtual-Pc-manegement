using Microsoft.Data.SqlClient;
using SVPM.Models;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;

public static class CustomersVirtualPCsRepository
{
    public static List<Mapping> Mappings { get; } = [];

    public static async Task GetAllMappingAsync()
    {
        Mappings.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.CustomersVirtualPcTable}";
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Mappings.Add(new Mapping
            {
                MappingId = reader.GetGuid(0),
                CustomerId = reader.GetGuid(1),
                VirtualPcId = reader.GetGuid(2),
                RecordState = RecordStates.Loaded,
                InDatabase = true
            });
        }
    }

    public static async Task AddMappingAsync(Mapping mapping)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        var query = $@"
                INSERT INTO {GlobalSettings.CustomersVirtualPcTable} (CustomerId, VirtualPcID)
                VALUES (@CustomerId, @VirtualPcID)";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", mapping.CustomerId);
        command.Parameters.AddWithValue("@VirtualPcID", mapping.VirtualPcId);
        await command.ExecuteNonQueryAsync();
        mapping.InDatabase = true;
        mapping.RecordState = RecordStates.Loaded;
    }

    public static async Task DeleteMappingAsync(Mapping mapping)
    {
        if (!mapping.InDatabase){Mappings.Remove(mapping); return; }
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var checkQuery = $@"SELECT * FROM {GlobalSettings.CustomersVirtualPcTable} WHERE MappingId = @MappingId";
            await using var checkCommand = new SqlCommand(checkQuery, connection, transaction as SqlTransaction);
            checkCommand.Parameters.AddWithValue("@MappingId", mapping.MappingId);
            await using var reader = await checkCommand.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await reader.CloseAsync();
                await transaction.RollbackAsync();
                Mappings.Remove(mapping);
                return;
            }
            await reader.CloseAsync();
            var deleteQuery = $"DELETE FROM {GlobalSettings.CustomersVirtualPcTable} WHERE MappingId = @MappingId";
            await using var deleteCommand = new SqlCommand(deleteQuery, connection, transaction as SqlTransaction);
            deleteCommand.Parameters.AddWithValue("@MappingId", mapping.MappingId);

            await deleteCommand.ExecuteNonQueryAsync();
            Mappings.Remove(mapping);
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}