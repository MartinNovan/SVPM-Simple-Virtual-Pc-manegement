using Microsoft.Data.SqlClient;

namespace SVPM;

public class CustomersVirtualPCsRepository
{
    private static readonly SqlConnection Connection = new(GlobalSettings.ConnectionString);
    public static List<Models.Mapping> MappingList { get; set; } = new();

    public static async Task GetAllMappingAsync()
    {
        var mappings = new List<Models.Mapping>();
        await Connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.CustomersVirtualPcTable}";
        var command = new SqlCommand(query, Connection);
        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            mappings.Add(new Models.Mapping
            {
                MappingID = reader.GetGuid(0),
                CustomerID = reader.GetGuid(1),
                VirtualPcID = reader.GetGuid(2),
                RecordState = Models.RecordStates.Loaded
            });
        }
        MappingList = mappings;
        await Connection.CloseAsync();
    }

    public static async Task AddMappingAsync(Models.Mapping mapping)
    {
        await Connection.OpenAsync();
        var query = $@"
            INSERT INTO {GlobalSettings.CustomersVirtualPcTable} (CustomerID, VirtualPcID)
            VALUES (@CustomerID, @VirtualPcID)";
        var command = new SqlCommand(query, Connection);
        command.Parameters.AddWithValue("@CustomerID", mapping.CustomerID);
        command.Parameters.AddWithValue("@VirtualPcID", mapping.VirtualPcID);
        await command.ExecuteNonQueryAsync();
        MappingList.Add(mapping);
    }

    public static async Task DeleteMappingAsync(Guid customerID, Guid virtualPcID)
    {
        Connection.Open();
        var transaction = Connection.BeginTransaction();
        try
        {
            var query = $@"
            DELETE FROM {GlobalSettings.CustomersVirtualPcTable}
            WHERE CustomerID = @CustomerID AND VirtualPcID = @VirtualPcID";
            var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@CustomerID", customerID);
            command.Parameters.AddWithValue("@VirtualPcID", virtualPcID);

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