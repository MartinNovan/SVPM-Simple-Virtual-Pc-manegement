using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;
public static class VirtualPcRepository
{
    public static ObservableCollection<VirtualPc> VirtualPcsList { get; set; } = [];

    public static async Task GetAllVirtualPCsAsync()
    {
        VirtualPcsList.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.VirtualPcTable}";
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            VirtualPcsList.Add(new VirtualPc
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
                RecordState = RecordStates.Loaded
            });
        }
        foreach (var virtualPc in VirtualPcsList)
        {
            virtualPc.OwningCustomers = new List<Customer>();
            foreach (var mapping in CustomersVirtualPCsRepository.MappingList.Where(m =>
                         m.VirtualPcID == virtualPc.VirtualPcID))
            {
                virtualPc.OwningCustomers.Add(
                    CustomerRepository.CustomersList.First(c => c.CustomerID == mapping.CustomerID));
            }
            virtualPc.InDatabase = true;
            virtualPc.InitializeOriginalValues();
        }
    }

    public static async Task AddVirtualPc(VirtualPc virtualPc)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            var query = $@"
            INSERT INTO {GlobalSettings.VirtualPcTable} (VirtualPcID, VirtualPcName, ServiceName, OperatingSystem, CPU_Cores, RAM_Size_GB, Disk_Size_GB, Backupping, Administration, IP_Address, FQDN, VirtualPcNotes)
            VALUES (@VirtualPcID, @VirtualPcName, @ServiceName, @OperatingSystem, @CPU_Cores, @RAM_Size_GB, @Disk_Size_GB, @Backupping, @Administration, @IP_Address, @FQDN, @Notes)";

            await using var command = new SqlCommand(query, connection);
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
            command.Parameters.AddWithValue("@Notes", virtualPc.Notes ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
            virtualPc.InDatabase = true;
            virtualPc.RecordState = RecordStates.Loaded;

        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding virtual PC: {ex.Message}", "OK");
        }
    }

    public static async Task DeleteVirtualPc(VirtualPc virtualPc)
    {
        if(!virtualPc.InDatabase) return;
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var selectQuery = $@"
            SELECT VirtualPcName, ServiceName, OperatingSystem, CPU_Cores, RAM_Size_GB, Disk_Size_GB, Backupping, Administration, IP_Address, FQDN, VirtualPcNotes
            FROM {GlobalSettings.VirtualPcTable}
            WHERE VirtualPcID = @VirtualPcID";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction as SqlTransaction);
            selectCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcID);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Virtual PC not found in the database.", "OK");
                return;
            }

            await reader.ReadAsync();
            var dbVirtualPcName = reader["VirtualPcName"] as string;
            var dbServiceName = reader["OperatingSystem"] as string;
            var dbCPU_Cores = (int)reader["CPU_Cores"];
            var dbRam_Size_GB = (int)reader["Ram_Size_GB"];
            var dbDisk_Size_GB = (int)reader["Disk_Size_GB"];
            var dbBackupping = (bool)reader["Backupping"];
            var dbAdministration = (bool)reader["Administration"];
            var dbIP_Address = reader["IP_Address"] as string;
            var dbFQDN = reader["IP_Address"] as string;
            var dbNotes = reader["VirtualPcNotes"] as string;

            reader.Close();

            if ((dbVirtualPcName != virtualPc.OriginalVirtualPcName) ||
                (dbServiceName != virtualPc.OriginalServiceName) ||
                (dbCPU_Cores != virtualPc.OriginalCPU_Cores) ||
                (dbRam_Size_GB != virtualPc.OriginalRAM_Size_GB) ||
                (dbDisk_Size_GB != virtualPc.OriginalDisk_Size_GB) ||
                (dbBackupping != virtualPc.OriginalBackupping) ||
                (dbAdministration != virtualPc.OriginalAdministration) ||
                (dbIP_Address != virtualPc.OriginalIP_Address) ||
                (dbFQDN != virtualPc.OriginalFQDN) ||
                (dbNotes != virtualPc.OriginalNotes))
            {
                bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Conflict Detected",
                    $"Database values have changed.\n\n"
                    + $"Current values in DB:\n"
                    + $"Virtual PC Name: {dbVirtualPcName}\nService Name: {dbServiceName}\n"
                    + $"CPU Cores: {dbCPU_Cores}\nRAM Size (GB): {dbRam_Size_GB}\n"
                    + $"Disk Size (GB): {dbDisk_Size_GB}\nBackupping: {dbBackupping}\n"
                    + $"Administration: {dbAdministration}\nIP Address: {dbIP_Address}\n"
                    + $"FQDN: {dbFQDN}\nNotes: {dbNotes}\n\n"
                    + $"Values you originally loaded:\n"
                    + $"Virtual PC Name: {virtualPc.OriginalVirtualPcName}\nService Name: {virtualPc.OriginalServiceName}\n"
                    + $"CPU Cores: {virtualPc.OriginalCPU_Cores}\nRAM Size (GB): {virtualPc.OriginalRAM_Size_GB}\n"
                    + $"Disk Size (GB): {virtualPc.OriginalDisk_Size_GB}\nBackupping: {virtualPc.OriginalBackupping}\n"
                    + $"Administration: {virtualPc.OriginalAdministration}\nIP Address: {virtualPc.OriginalIP_Address}\n"
                    + $"FQDN: {virtualPc.OriginalFQDN}\nNotes: {virtualPc.OriginalNotes}\n\n"
                    + "Do you still want to delete this virtual PC from the database?",
                    "Yes", "No");

                if (!confirm)
                {
                    return;
                }
            }


            var deleteQuery = $"DELETE FROM {GlobalSettings.VirtualPcTable} WHERE VirtualPcID = @VirtualPcID";
            await using (var deleteCommand = new SqlCommand(deleteQuery, connection, transaction as SqlTransaction))
            {
                deleteCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcID);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            var checkQuery = $"SELECT COUNT(*) FROM {GlobalSettings.CustomersVirtualPcTable} WHERE VirtualPcID = @VirtualPcID";
            await using (var checkCommand = new SqlCommand(checkQuery, connection, transaction as SqlTransaction))
            {
                checkCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcID);
                var count = (int)(await checkCommand.ExecuteScalarAsync() ?? 0);

                if (count > 0)
                {
                    throw new Exception("Some linked records were not deleted properly.");
                }
            }
            await transaction.CommitAsync();
            VirtualPcsList.Remove(virtualPc);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting virtual PC: {ex.Message}", "OK");
        }
    }

    public static async Task UpdateVirtualPc(VirtualPc virtualPc)
    {
        if (!virtualPc.InDatabase)
        {
            await AddVirtualPc(virtualPc);
            return;
        }
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var selectQuery = $@"
            SELECT VirtualPcName, ServiceName, OperatingSystem, CPU_Cores, RAM_Size_GB, Disk_Size_GB, Backupping, Administration, IP_Address, FQDN, VirtualPcNotes
            FROM {GlobalSettings.VirtualPcTable}
            WHERE VirtualPcID = @VirtualPcID";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction as SqlTransaction);
            selectCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcID);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Virtual PC not found in the database.", "OK");
                return;
            }

            await reader.ReadAsync();
            var dbVirtualPcName = reader["VirtualPcName"] as string;
            var dbServiceName = reader["OperatingSystem"] as string;
            var dbCPU_Cores = (int)reader["CPU_Cores"];
            var dbRam_Size_GB = (int)reader["Ram_Size_GB"];
            var dbDisk_Size_GB = (int)reader["Disk_Size_GB"];
            var dbBackupping = (bool)reader["Backupping"];
            var dbAdministration = (bool)reader["Administration"];
            var dbIP_Address = reader["IP_Address"] as string;
            var dbFQDN = reader["IP_Address"] as string;
            var dbNotes = reader["VirtualPcNotes"] as string;

            reader.Close();

            if ((dbVirtualPcName != virtualPc.OriginalVirtualPcName) ||
                (dbServiceName != virtualPc.OriginalServiceName) ||
                (dbCPU_Cores != virtualPc.OriginalCPU_Cores) ||
                (dbRam_Size_GB != virtualPc.OriginalRAM_Size_GB) ||
                (dbDisk_Size_GB != virtualPc.OriginalDisk_Size_GB) ||
                (dbBackupping != virtualPc.OriginalBackupping) ||
                (dbAdministration != virtualPc.OriginalAdministration) ||
                (dbIP_Address != virtualPc.OriginalIP_Address) ||
                (dbFQDN != virtualPc.OriginalFQDN) ||
                (dbNotes != virtualPc.OriginalNotes))
            {
                bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Conflict Detected",
                    $"Database values have changed.\n\n"
                    + $"Current values in DB:\n"
                    + $"Virtual PC Name: {dbVirtualPcName}\nService Name: {dbServiceName}\n"
                    + $"CPU Cores: {dbCPU_Cores}\nRAM Size (GB): {dbRam_Size_GB}\n"
                    + $"Disk Size (GB): {dbDisk_Size_GB}\nBackupping: {dbBackupping}\n"
                    + $"Administration: {dbAdministration}\nIP Address: {dbIP_Address}\n"
                    + $"FQDN: {dbFQDN}\nNotes: {dbNotes}\n\n"
                    + $"Values you originally loaded:\n"
                    + $"Virtual PC Name: {virtualPc.OriginalVirtualPcName}\nService Name: {virtualPc.OriginalServiceName}\n"
                    + $"CPU Cores: {virtualPc.OriginalCPU_Cores}\nRAM Size (GB): {virtualPc.OriginalRAM_Size_GB}\n"
                    + $"Disk Size (GB): {virtualPc.OriginalDisk_Size_GB}\nBackupping: {virtualPc.OriginalBackupping}\n"
                    + $"Administration: {virtualPc.OriginalAdministration}\nIP Address: {virtualPc.OriginalIP_Address}\n"
                    + $"FQDN: {virtualPc.OriginalFQDN}\nNotes: {virtualPc.OriginalNotes}\n\n"
                    + "Do you want to rewrite this virtual PC in the database?",
                    "Yes", "No");

                if (!confirm)
                {
                    return;
                }
            }

            var updateQuery = $@"
            UPDATE {GlobalSettings.VirtualPcTable}
            SET VirtualPcName = @VirtualPcName, 
                ServiceName = @ServiceName, 
                OperatingSystem = @OperatingSystem, 
                CPU_Cores = @CPU_Cores, 
                RAM_Size_GB = @RAM_Size_GB, 
                Disk_Size_GB = @Disk_Size_GB, 
                Backupping = @Backupping, 
                Administration = @Administration, 
                IP_Address = @IP_Address, 
                FQDN = @FQDN, 
                VirtualPcNotes = @VirtualPcNotes
            WHERE VirtualPcID = @VirtualPcID";

            await using var updateCommand = new SqlCommand(updateQuery, connection, transaction as SqlTransaction);
            updateCommand.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);
            updateCommand.Parameters.AddWithValue("@ServiceName", virtualPc.ServiceName);
            updateCommand.Parameters.AddWithValue("@OperatingSystem", virtualPc.OperatingSystem);
            updateCommand.Parameters.AddWithValue("@CPU_Cores", virtualPc.CPU_Cores);
            updateCommand.Parameters.AddWithValue("@RAM_Size_GB", virtualPc.RAM_Size_GB);
            updateCommand.Parameters.AddWithValue("@Disk_Size_GB", virtualPc.Disk_Size_GB);
            updateCommand.Parameters.AddWithValue("@Backupping", virtualPc.Backupping);
            updateCommand.Parameters.AddWithValue("@Administration", virtualPc.Administration);
            updateCommand.Parameters.AddWithValue("@IP_Address", virtualPc.IP_Address ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@FQDN", virtualPc.FQDN ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@VirtualPcNotes", virtualPc.Notes ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcID);


            await updateCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            virtualPc.RecordState = RecordStates.Loaded;
            virtualPc.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when updating virtual PC: {ex.Message}", "OK");
        }
    }
}
