using System.Data;
using Microsoft.Data.SqlClient;
using SVPM.Models;
using SVPM.ViewModels;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace SVPM.Repositories;

public static class LogRepository
{
    private static async Task GetCustomerLogs()
    {
        CustomerLogViewModel.CustomersLogs.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        await using var getCommand = new SqlCommand("GetCustomersLogs", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var customerLog = new CustomerLog
            {
                AuditId = reader.GetGuid(reader.GetOrdinal("AuditId")),
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
                OperationType = reader.GetString(reader.GetOrdinal("OperationType")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                CustomerTag = reader.GetString(reader.GetOrdinal("CustomerTag")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                ChangedBy = reader.GetString(reader.GetOrdinal("ChangedBy")),
            };
            CustomerLogViewModel.CustomersLogs.Add(customerLog);
        }
    }

    private static async Task GetVirtualPcLogs()
    {
        VirtualPcLogViewModel.VirtualPcsLogs.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        await using var getCommand = new SqlCommand("GetVirtualPcsLogs", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var virtualPcLog = new VirtualPcLog
            {
                AuditId = reader.GetGuid(reader.GetOrdinal("AuditId")),
                VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcID")),
                OperationType = reader.GetString(reader.GetOrdinal("OperationType")),
                VirtualPcName = reader.GetString(reader.GetOrdinal("VirtualPcName")),
                Service = reader.IsDBNull(reader.GetOrdinal("Service"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("Service")),
                OperatingSystem = reader.IsDBNull(reader.GetOrdinal("OperatingSystem"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("OperatingSystem")),
                CpuCores = reader.IsDBNull(reader.GetOrdinal("CpuCores"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("CpuCores")),
                RamSize = reader.IsDBNull(reader.GetOrdinal("RamSize"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("RamSize")),
                DiskSize = reader.IsDBNull(reader.GetOrdinal("DiskSize"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("DiskSize")),
                Backupping = reader.GetBoolean(reader.GetOrdinal("Backupping")),
                Administration = reader.GetBoolean(reader.GetOrdinal("Administration")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("IpAddress")),
                Fqdn = reader.IsDBNull(reader.GetOrdinal("Fqdn")) ? "" : reader.GetString(reader.GetOrdinal("Fqdn")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                ChangedBy = reader.GetString(reader.GetOrdinal("ChangedBy")),
            };

            VirtualPcLogViewModel.VirtualPcsLogs.Add(virtualPcLog);
        }
    }

    private static async Task GetMappingLogs()
    {
        MappingLogViewModel.MappingsLogs.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();

        await using var getCommand = new SqlCommand("GetMappingsLogs", connection);
        getCommand.CommandType = CommandType.StoredProcedure;

        await using var reader = await getCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var mappingLog = new MappingLog
            {
                AuditId = reader.GetGuid(reader.GetOrdinal("AuditId")),
                MappingId = reader.GetGuid(reader.GetOrdinal("MappingID")),
                CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerID")),
                VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcID")),
                OperationType = reader.GetString(reader.GetOrdinal("OperationType")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                ChangedBy = reader.GetString(reader.GetOrdinal("ChangedBy")),
            };
            MappingLogViewModel.MappingsLogs.Add(mappingLog);
        }
    }

    private static async Task GetAccountsLogs()
    {
        AccountLogViewModel.AccountsLogs.Clear();
        await using var connection = new SqlConnection(GlobalSettings.ConnectionString);
        await connection.OpenAsync();
        
        await using var getCommand = new SqlCommand("GetAccountsLogs", connection);
        getCommand.CommandType = CommandType.StoredProcedure;
        
        await using var reader = await getCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var accountLog = new AccountLog
            {
                AuditId = reader.GetGuid(reader.GetOrdinal("AuditId")),
                AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                OperationType = reader.GetString(reader.GetOrdinal("OperationType")),
                AssociatedVirtualPc = new VirtualPc { VirtualPcId = reader.GetGuid(reader.GetOrdinal("VirtualPcId")) },
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Password = reader.IsDBNull(reader.GetOrdinal("Password"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("Password")),
                BackupPassword = reader.IsDBNull(reader.GetOrdinal("BackupPassword"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("BackupPassword")),
                Admin = reader.GetBoolean(reader.GetOrdinal("Admin")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated")),
                VerifyHash = reader.GetString(reader.GetOrdinal("VerifyHash")),
                ChangedBy = reader.GetString(reader.GetOrdinal("ChangedBy")),

            };
            AccountLogViewModel.AccountsLogs.Add(accountLog);
        }
    }
    
    public static async Task GetAllLogs()
    {
        await GetCustomerLogs();
        await GetVirtualPcLogs();
        await GetMappingLogs();
        await GetAccountsLogs();
    }
}