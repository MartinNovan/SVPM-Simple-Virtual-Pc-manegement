using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace SVPM_Starlit_Virtual_Pc_Manegement;
public static class VirtualPcRepository
{
    private static readonly SqlConnection Connection = new(GlobalSettings.ConnectionString);
    public static ObservableCollection<Models.VirtualPC> VirtualPcsList { get; set; } = new();
    public static async Task GetAllVirtualPCsAsync()
    {
        VirtualPcsList.Clear();
        await Connection.OpenAsync();
        
        var query = $"SELECT * FROM {GlobalSettings.VirtualPcTable}";
        var command = new SqlCommand(query, Connection);
        var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            VirtualPcsList.Add(new Models.VirtualPC
            {
                VirtualPcID = reader.GetGuid(0),
                VirtualPcName = reader.GetString(1),
                ServiceName = reader.GetString(2),
                OperatingSystem = reader.GetString(3),
                CPU_Cores = reader.GetInt32(4),
                RAM_Size_GB = reader.GetInt32(5),
                Disk_Size_GB = reader.GetInt32(6),
                Backupping = reader.GetBoolean(7),
                Administration = reader.GetBoolean(8),
                IP_Address = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                FQDN = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                Notes = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                RecordState = Models.RecordStates.Loaded
            });
        }
        await Connection.CloseAsync();
        try
        {
            foreach (var virtualPc in VirtualPcsList)
            {
                virtualPc.OwningCustomers = new List<Models.Customer>();
                foreach (var mapping in CustomersVirtualPCsRepository.MappingList.Where(m =>
                             m.VirtualPcID == virtualPc.VirtualPcID))
                {
                    virtualPc.OwningCustomers.Add(
                        CustomerRepository.CustomersList.First(c => c.CustomerID == mapping.CustomerID));
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error while mapping customer to virtual PCs: {ex.Message}", "OK");
        }
    }

    public static async Task AddVirtualPc(Models.VirtualPC virtualPc)
    {
        try
        {
            Connection.Open();

            var query = $@"
            INSERT INTO {GlobalSettings.VirtualPcTable} 
            (VirtualPcName, ServiceName, OperatingSystem, CPU_Cores, RAM_Size_GB, Disk_Size_GB, Backupping, Administration, IP_Address, FQDN, VirtualPcNotes)
            VALUES 
            (@VirtualPcName, @ServiceName, @OperatingSystem, @CPU_Cores, @RAM_Size_GB, @Disk_Size_GB, @Backupping, @Administration, @IP_Address, @FQDN, @Notes)";

            using var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);
            command.Parameters.AddWithValue("@ServiceName", virtualPc.ServiceName);
            command.Parameters.AddWithValue("@OperatingSystem", virtualPc.OperatingSystem);
            command.Parameters.AddWithValue("@CPU_Cores", virtualPc.CPU_Cores);
            command.Parameters.AddWithValue("@RAM_Size_GB", virtualPc.RAM_Size_GB);
            command.Parameters.AddWithValue("@Disk_Size_GB", virtualPc.Disk_Size_GB);
            command.Parameters.AddWithValue("@Backupping", virtualPc.Backupping);
            command.Parameters.AddWithValue("@Administration", virtualPc.Administration);
            command.Parameters.AddWithValue("@IP_Address", virtualPc.IP_Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FQDN", virtualPc.FQDN ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", virtualPc.Notes ?? (object)DBNull.Value);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public static async Task DeleteVirtualPc(Guid virtualPcId)
    {
        Connection.Open();
        await using var transaction = Connection.BeginTransaction();
        try
        {
            var query = $"DELETE FROM {GlobalSettings.VirtualPcTable} WHERE VirtualPcID = @VirtualPcID";
            using var virtualPcCommand = new SqlCommand(query, Connection);
            virtualPcCommand.Parameters.AddWithValue("@VirtualPcID", virtualPcId);
            virtualPcCommand.ExecuteNonQuery();

            var mappingQuery = $"DELETE FROM {GlobalSettings.CustomersVirtualPcTable} WHERE VirtualPcID = @VirtualPcID";
            using var mappingCommand = new SqlCommand(mappingQuery, Connection);
            mappingCommand.Parameters.AddWithValue("@VirtualPcID", virtualPcId);
            mappingCommand.ExecuteNonQuery();

            var accountQuery = $"DELETE FROM {GlobalSettings.AccountTable} WHERE VirtualPcID = @VirtualPcID";
            using var accountCommand = new SqlCommand(accountQuery, Connection);
            accountCommand.Parameters.AddWithValue("@VirtualPcID", virtualPcId);
            accountCommand.ExecuteNonQuery();

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


    public static async Task UpdateVirtualPc(Models.VirtualPC virtualPc)
    {
        Connection.Open();
        var transaction = Connection.BeginTransaction();
        try
        {
            var query = $@"
            UPDATE {GlobalSettings.VirtualPcTable}
            SET VirtualPcName = @VirtualPcName, ServiceName = @ServiceName, OperatingSystem = @OperatingSystem, CPU_Cores = @CPU_Cores, RAM_Size_GB = @RAM_Size_GB, Disk_Size_GB = @Disk_Size_GB, Backupping = @Backupping, Administration = @Administration, IP_Address = @IP_Address, FQDN = @FQDN
            WHERE VirtualPcID = @VirtualPcID";

            using var command = new SqlCommand(query, Connection);
            command.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcID);
            command.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);
            command.Parameters.AddWithValue("@ServiceName", virtualPc.ServiceName);
            command.Parameters.AddWithValue("@OperatingSystem", virtualPc.OperatingSystem);
            command.Parameters.AddWithValue("@CPU_Cores", virtualPc.CPU_Cores);
            command.Parameters.AddWithValue("@RAM_Size_GB", virtualPc.RAM_Size_GB);
            command.Parameters.AddWithValue("@Disk_Size_GB", virtualPc.Disk_Size_GB);
            command.Parameters.AddWithValue("@Backupping", virtualPc.Backupping);
            command.Parameters.AddWithValue("@Administration", virtualPc.Administration);
            command.Parameters.AddWithValue("@IP_Address", virtualPc.IP_Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FQDN", virtualPc.FQDN ?? (object)DBNull.Value);

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
