using System.Data;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;

public static class CustomersVirtualPCsRepository
{
    public static async Task<List<Mapping>> GetAllMappingAsync()
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        await using var getCommand = new SqlCommand("GetMappings", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();
        
        var mappings = new List<Mapping>();
        while (await reader.ReadAsync())
        {
            mappings.Add(new Mapping
            {
                MappingId = reader.GetGuid(reader.GetOrdinal("MappingId")),
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
                VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcId")),
                RecordState = RecordStates.Loaded,
                InDatabase = true
            });
        }
        return mappings;
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
    }

    public static async Task DeleteMappingAsync(Mapping mapping)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            var command = new SqlCommand("DeleteMapping", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@MappingId", mapping.MappingId);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}