using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using static SVPM.Repositories.CustomerRepository;
using static SVPM.Repositories.CustomersVirtualPCsRepository;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;
public static class VirtualPcRepository
{
    public static ObservableCollection<VirtualPc> VirtualPCs { get; } = [];
    public static async Task GetAllVirtualPCsAsync()
    {
        VirtualPCs.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        var query = $"SELECT * FROM {GlobalSettings.VirtualPcTable}";
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            VirtualPCs.Add(new VirtualPc
            {
                VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcID")),
                VirtualPcName = reader.GetString(reader.GetOrdinal("VirtualPcName")),
                Service = reader.GetString(reader.GetOrdinal("Service")),
                OperatingSystem = reader.GetString(reader.GetOrdinal("OperatingSystem")),
                CpuCores = reader.GetString(reader.GetOrdinal("CpuCores")),
                RamSize = reader.GetString(reader.GetOrdinal("RamSize")),
                DiskSize = reader.GetString(reader.GetOrdinal("DiskSize")),
                Backupping = reader.GetBoolean(reader.GetOrdinal("Backupping")),
                Administration = reader.GetBoolean(reader.GetOrdinal("Administration")),
                IpAddress = reader.GetString(reader.GetOrdinal("IpAddress")),
                Fqdn = reader.GetString(reader.GetOrdinal("Fqdn")),
                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                RecordState = RecordStates.Loaded
            });
        }
        foreach (var virtualPc in VirtualPCs)
        {
            virtualPc.OwningCustomers = new ObservableCollection<Customer>();
            foreach (var mapping in Mappings.Where(m => m.VirtualPcId == virtualPc.VirtualPcId))
            {
                virtualPc.OwningCustomers.Add(Customers.First(c => c.CustomerId == mapping.CustomerId));
            }
            virtualPc.InitializeOriginalValues();
            virtualPc.SetOwningCustomersNames();
        }
    }

    public static async Task AddVirtualPc(VirtualPc virtualPc)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            var conflict = await LookForConflict(virtualPc, connection);
            if (conflict) return;

            var query = $@"
            INSERT INTO {GlobalSettings.VirtualPcTable} (VirtualPcId, VirtualPcName, Service, OperatingSystem, CpuCores, RamSize, DiskSize, Backupping, Administration, IpAddress, Fqdn, Notes, Updated, VerifyHash)
            VALUES (@VirtualPcId, @VirtualPcName, @Service, @OperatingSystem, @CpuCores, @RamSize, @DiskSize, @Backupping, @Administration, @IpAddress, @Fqdn, @Notes, @Updated, @VerifyHash)";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VirtualPcId", virtualPc.VirtualPcId);
            command.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);
            command.Parameters.AddWithValue("@Service", virtualPc.Service ?? String.Empty);
            command.Parameters.AddWithValue("@OperatingSystem", virtualPc.OperatingSystem ?? String.Empty);
            command.Parameters.AddWithValue("@CpuCores", virtualPc.CpuCores ?? String.Empty);
            command.Parameters.AddWithValue("@RamSize", virtualPc.RamSize ?? String.Empty);
            command.Parameters.AddWithValue("@DiskSize", virtualPc.DiskSize ?? String.Empty);
            command.Parameters.AddWithValue("@Backupping", virtualPc.Backupping);
            command.Parameters.AddWithValue("@Administration", virtualPc.Administration);
            command.Parameters.AddWithValue("@IpAddress", virtualPc.IpAddress ?? String.Empty);
            command.Parameters.AddWithValue("@Fqdn", virtualPc.Fqdn ?? String.Empty);
            command.Parameters.AddWithValue("@Notes", virtualPc.Notes ?? String.Empty);
            command.Parameters.AddWithValue("@Updated", virtualPc.Updated);
            command.Parameters.AddWithValue("@VerifyHash", virtualPc.VerifyHash);

            await command.ExecuteNonQueryAsync();
            virtualPc.RecordState = RecordStates.Loaded;
            virtualPc.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when adding virtual PC: {ex.Message}", "OK");
        }
    }

    public static async Task DeleteVirtualPc(VirtualPc virtualPc)
    {
        if(virtualPc.OriginalRecordState != RecordStates.Loaded) { VirtualPCs.Remove(virtualPc); return; }
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var isChange = await LookForChange(virtualPc, connection, transaction as SqlTransaction);
            if (isChange) return;

            var deleteQuery = $"DELETE FROM {GlobalSettings.VirtualPcTable} WHERE VirtualPcId = @VirtualPcId";
            await using (var deleteCommand = new SqlCommand(deleteQuery, connection, transaction as SqlTransaction))
            {
                deleteCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcId);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            var checkQuery = $"SELECT COUNT(*) FROM {GlobalSettings.CustomersVirtualPcTable} WHERE VirtualPcID = @VirtualPcID";
            await using (var checkCommand = new SqlCommand(checkQuery, connection, transaction as SqlTransaction))
            {
                checkCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcId);
                var count = (int)(await checkCommand.ExecuteScalarAsync() ?? 0);

                if (count > 0)
                {
                    await Application.Current?.Windows[0].Page?.DisplayAlert("Error", "Some mappings weren't properly deleted!", "OK")!;
                }
            }

            var checkQuery2 = $"SELECT COUNT(*) FROM {GlobalSettings.AccountTable} WHERE VirtualPcId = @VirtualPcId";
            await using (var checkCommand2 = new SqlCommand(checkQuery2, connection, transaction as SqlTransaction))
            {
                checkCommand2.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcId);
                var count = (int)(await checkCommand2.ExecuteScalarAsync() ?? 0);

                if (count > 0)
                {
                    await Application.Current?.Windows[0].Page?.DisplayAlert("Error", "Some accounts weren't properly deleted!", "OK")!;
                }
            }
            await transaction.CommitAsync();
            VirtualPCs.Remove(virtualPc);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when deleting virtual PC: {ex.Message}", "OK");
        }
    }

    public static async Task UpdateVirtualPc(VirtualPc virtualPc)
    {
        if (virtualPc.OriginalRecordState != RecordStates.Loaded)
        {
            await AddVirtualPc(virtualPc);
            return;
        }
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var isChange = await LookForChange(virtualPc, connection, transaction as SqlTransaction);
            if (isChange) return;

            var conflict = await LookForConflict(virtualPc, connection, transaction as SqlTransaction);
            if (conflict) return;

            var updateQuery = $@"UPDATE {GlobalSettings.VirtualPcTable}
            SET VirtualPcName = @VirtualPcName, 
                Service = @Service, 
                OperatingSystem = @OperatingSystem, 
                CpuCores = @CpuCores, 
                RamSize = @RamSize, 
                DiskSize = @DiskSize, 
                Backupping = @Backupping, 
                Administration = @Administration, 
                IpAddress = @IpAddress, 
                Fqdn = @Fqdn, 
                Notes = @Notes,
                Updated = @Updated,
                VerifyHash = @VerifyHash
            WHERE VirtualPcId = @VirtualPcId";

            await using var updateCommand = new SqlCommand(updateQuery, connection, transaction as SqlTransaction);
            updateCommand.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);
            updateCommand.Parameters.AddWithValue("@Service", virtualPc.Service ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@OperatingSystem", virtualPc.OperatingSystem ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@CpuCores", virtualPc.CpuCores ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@RamSize", virtualPc.RamSize ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@DiskSize", virtualPc.DiskSize ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Backupping", virtualPc.Backupping);
            updateCommand.Parameters.AddWithValue("@Administration", virtualPc.Administration);
            updateCommand.Parameters.AddWithValue("@IpAddress", virtualPc.IpAddress ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Fqdn", virtualPc.Fqdn ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Notes", virtualPc.Notes ?? String.Empty);
            updateCommand.Parameters.AddWithValue("@Updated", virtualPc.Updated);
            updateCommand.Parameters.AddWithValue("@VerifyHash", virtualPc.VerifyHash);
            updateCommand.Parameters.AddWithValue("@VirtualPcId", virtualPc.VirtualPcId);


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

    private static async Task<bool> LookForChange(VirtualPc virtualPc, SqlConnection connection, SqlTransaction? transaction = null)
    {
        try
        {
            var selectQuery = $"SELECT VerifyHash FROM {GlobalSettings.VirtualPcTable} WHERE VirtualPcID = @VirtualPcID";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction);
            selectCommand.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcId);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Virtual PC not found in the database.", "OK");
                return true;
            }

            await reader.ReadAsync();
            var dbVerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash"));

            reader.Close();
            if (dbVerifyHash != virtualPc.OriginalVerifyHash)
            {
                var selectQuery2 = $@"
                    SELECT VirtualPcName, Service, OperatingSystem, CPU_Cores, RAM_Size_GB, Disk_Size_GB, Backupping, Administration, IP_Address, FQDN, VirtualPcNotes
                    FROM {GlobalSettings.VirtualPcTable}
                    WHERE VirtualPcID = @VirtualPcID";

                await using var selectCommand2 = new SqlCommand(selectQuery2, connection, transaction);
                selectCommand2.Parameters.AddWithValue("@VirtualPcID", virtualPc.VirtualPcId);

                await using var reader3 = await selectCommand2.ExecuteReaderAsync();
                if (!reader3.HasRows)
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Virtual PC not found in the database.", "OK");
                    return true;
                }

                await reader3.ReadAsync();
                var dbVirtualPcName = reader3["VirtualPcName"] as string;
                var dbServiceName = reader3["Service"] as string;
                var dbOperatingSystem = reader3["OperatingSystem"] as string;
                var dbCpuCores = reader3["CPU_Cores"];
                var dbRamSize = reader3["Ram_Size_GB"];
                var dbDiskSize = reader3["Disk_Size_GB"];
                var dbBackupping = (bool)reader3["Backupping"];
                var dbAdministration = (bool)reader3["Administration"];
                var dbIpAddress = reader3["IP_Address"] as string;
                var dbFqdn = reader3["IP_Address"] as string;
                var dbNotes = reader3["VirtualPcNotes"] as string;

                reader3.Close();

                bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Conflict Detected",
                    $"Database values have changed.\n\n"
                    + $"Current values in DB:\n"
                    + $"Virtual PC Name: {dbVirtualPcName}\nService Name: {dbServiceName}\n Operating System: {dbOperatingSystem}\n"
                    + $"CPU Cores: {dbCpuCores}\nRAM Size (GB): {dbRamSize}\n"
                    + $"Disk Size (GB): {dbDiskSize}\nBackupping: {dbBackupping}\n"
                    + $"Administration: {dbAdministration}\nIP Address: {dbIpAddress}\n"
                    + $"FQDN: {dbFqdn}\nNotes: {dbNotes}\n\n"
                    + $"Your current values:\n"
                    + $"Virtual PC Name: {virtualPc.VirtualPcName}\nService Name: {virtualPc.Service}\n Operating System: {virtualPc.OperatingSystem}\n"
                    + $"CPU Cores: {virtualPc.CpuCores}\nRAM Size (GB): {virtualPc.RamSize}\n"
                    + $"Disk Size (GB): {virtualPc.DiskSize}\nBackupping: {virtualPc.Backupping}\n"
                    + $"Administration: {virtualPc.Administration}\nIP Address: {virtualPc.IpAddress}\n"
                    + $"FQDN: {virtualPc.Fqdn}\nNotes: {virtualPc.Notes}\n\n"
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

    private static async Task<bool> LookForConflict(VirtualPc virtualPc, SqlConnection connection, SqlTransaction? transaction = null)
    {
        try
        {
            var selectQuery = $"SELECT * FROM {GlobalSettings.VirtualPcTable} WHERE VirtualPcName = @VirtualPcName";

            await using var selectCommand = new SqlCommand(selectQuery, connection, transaction);
            selectCommand.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            if (reader.HasRows && virtualPc.VirtualPcName != virtualPc.OriginalVirtualPcName)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", "Virtual PC with this name was already added to database.", "OK");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when looking for conflict:{ex.Message}","OK");
            return true;
        }
    }
}
