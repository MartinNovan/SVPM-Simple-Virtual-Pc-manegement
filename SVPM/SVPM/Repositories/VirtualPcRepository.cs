using System.Collections.ObjectModel;
using System.Data;
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

        await using var getCommand = new SqlCommand("GetVirtualPCs", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var virtualPc = new VirtualPc
            {
                VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcID")),
                VirtualPcName = reader.GetString(reader.GetOrdinal("VirtualPcName")),
                Service = reader.IsDBNull(reader.GetOrdinal("Service")) ? "" : reader.GetString(reader.GetOrdinal("Service")),
                OperatingSystem = reader.IsDBNull(reader.GetOrdinal("OperatingSystem")) ? "" : reader.GetString(reader.GetOrdinal("OperatingSystem")),
                CpuCores = reader.IsDBNull(reader.GetOrdinal("CpuCores")) ? "" : reader.GetString(reader.GetOrdinal("CpuCores")),
                RamSize = reader.IsDBNull(reader.GetOrdinal("RamSize")) ? "" : reader.GetString(reader.GetOrdinal("RamSize")),
                DiskSize = reader.IsDBNull(reader.GetOrdinal("DiskSize")) ? "" : reader.GetString(reader.GetOrdinal("DiskSize")),
                Backupping = reader.GetBoolean(reader.GetOrdinal("Backupping")),
                Administration = reader.GetBoolean(reader.GetOrdinal("Administration")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? "" : reader.GetString(reader.GetOrdinal("IpAddress")),
                Fqdn = reader.IsDBNull(reader.GetOrdinal("Fqdn")) ? "" : reader.GetString(reader.GetOrdinal("Fqdn")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                RecordState = RecordStates.Loaded,
            };
            virtualPc.InitializeOriginalValues();
            VirtualPCs.Add(virtualPc);
        }
    }

    public static async Task AddVirtualPc(VirtualPc virtualPc)
    {
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        try
        {
            await using var addCommand = new SqlCommand("AddVirtualPc", connection);
            addCommand.CommandType = CommandType.StoredProcedure;
            addCommand.Parameters.AddWithValue("@VirtualPcId", virtualPc.VirtualPcId);
            addCommand.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);
            addCommand.Parameters.AddWithValue("@Service", virtualPc.Service  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@OperatingSystem", virtualPc.OperatingSystem  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@CpuCores", virtualPc.CpuCores  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@RamSize", virtualPc.RamSize  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@DiskSize", virtualPc.DiskSize  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@Backupping", virtualPc.Backupping);
            addCommand.Parameters.AddWithValue("@Administration", virtualPc.Administration);
            addCommand.Parameters.AddWithValue("@IpAddress", virtualPc.IpAddress  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@Fqdn", virtualPc.Fqdn  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@Notes", virtualPc.Notes  ?? (object)DBNull.Value);
            addCommand.Parameters.AddWithValue("@VerifyHash", virtualPc.VerifyHash);

            await addCommand.ExecuteNonQueryAsync();
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
        try
        {
            var isChange = await LookForChange(virtualPc, connection);
            if (isChange) return;

            await using var deleteCommand = new SqlCommand("DeleteVirtualPc", connection);
            deleteCommand.CommandType = CommandType.StoredProcedure;
            deleteCommand.Parameters.AddWithValue("@VirtualPcId", virtualPc.VirtualPcId);
            await deleteCommand.ExecuteNonQueryAsync();

            VirtualPCs.Remove(virtualPc);
        }
        catch (Exception ex)
        {
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

        try
        {
            var isChange = await LookForChange(virtualPc, connection);
            if (isChange) return;

            await using var updateCommand = new SqlCommand("UpdateVirtualPc", connection);
            updateCommand.CommandType = CommandType.StoredProcedure;
            updateCommand.Parameters.AddWithValue("@VirtualPcName", virtualPc.VirtualPcName);
            updateCommand.Parameters.AddWithValue("@Service", virtualPc.Service  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@OperatingSystem", virtualPc.OperatingSystem  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@CpuCores", virtualPc.CpuCores  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@RamSize", virtualPc.RamSize  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@DiskSize", virtualPc.DiskSize  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Backupping", virtualPc.Backupping);
            updateCommand.Parameters.AddWithValue("@Administration", virtualPc.Administration);
            updateCommand.Parameters.AddWithValue("@IpAddress", virtualPc.IpAddress  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Fqdn", virtualPc.Fqdn  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@Notes", virtualPc.Notes  ?? (object)DBNull.Value);
            updateCommand.Parameters.AddWithValue("@VerifyHash", virtualPc.VerifyHash);
            updateCommand.Parameters.AddWithValue("@VirtualPcId", virtualPc.VirtualPcId);

            await updateCommand.ExecuteNonQueryAsync();
            virtualPc.RecordState = RecordStates.Loaded;
            virtualPc.InitializeOriginalValues();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when updating virtual PC: {ex.Message}", "OK");
        }
    }

    private static async Task<bool> LookForChange(VirtualPc virtualPc, SqlConnection connection)
    {
        try
        {
            await using var checkCommand = new SqlCommand("CheckForVirtualPcConflict", connection);
            checkCommand.CommandType = CommandType.StoredProcedure;
            checkCommand.Parameters.AddWithValue("@VirtualPcId", virtualPc.VirtualPcId);
            checkCommand.Parameters.AddWithValue("@OriginalVerifyHash", virtualPc.OriginalVerifyHash);

            await using var reader = await checkCommand.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync();

                var dbVirtualPcName = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbServiceName = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbOperatingSystem = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbCpuCores = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbRamSize = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbDiskSize = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbBackupping = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbAdministration = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbIpAddress = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbFqdn = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                var dbNotes = reader.GetString(reader.GetOrdinal("VirtualPcName"));
                reader.Close();

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

                return !confirm;
            }
            await reader.CloseAsync();
            return false;
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error when looking for change:{ex.Message}","OK");
            return true;
        }
    }
}
