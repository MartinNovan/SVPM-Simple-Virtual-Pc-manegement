using System.Data;
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

        await using var getCommand = new SqlCommand("GetMappings", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Mappings.Add(new Mapping
            {
                MappingId = reader.GetGuid(reader.GetOrdinal("MappingId")),
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
                VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcId")),
                RecordState = RecordStates.Loaded,
                InDatabase = true
            });
        }
    }

    public static async Task AddMappingAsync(Mapping mapping)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        await using var addCommand = new SqlCommand("AddMapping", connection);
        addCommand.CommandType = CommandType.StoredProcedure;
        addCommand.Parameters.AddWithValue("@CustomerId", mapping.CustomerId);
        addCommand.Parameters.AddWithValue("@VirtualPcID", mapping.VirtualPcId);

        await addCommand.ExecuteNonQueryAsync();
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
            var command = new SqlCommand("DeleteMapping", connection, transaction as SqlTransaction);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@MappingId", mapping.MappingId);

            await command.ExecuteNonQueryAsync();
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