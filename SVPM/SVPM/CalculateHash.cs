using System.Security.Cryptography;
using System.Text;
using SVPM.Models;
using SVPM.Services;

namespace SVPM;

public static class CalculateHash
{
    public static async Task<string> CalculateVerifyHash(Customer? customer = null, VirtualPc? virtualPc = null, Account? account = null)
    {
        try
        {
            string verifyHash = String.Empty;
            if (customer == null && virtualPc == null && account == null)
            {
                return verifyHash;
            }

            switch (customer, virtualPc, account)
            {
                case (not null, null, null):
                    var vpc = await CustomerService.Instance.GetCustomerVirtualPCs(customer);
                    verifyHash = customer.FullName + customer.CustomerTag + customer.Email + customer.Phone + customer.Notes + string.Join(",", vpc.Select(v => v?.VirtualPcName));
                    break;
                case (null, not null, null):
                    var customers= await VirtualPcService.Instance.GetCustomerNamesAsString(virtualPc);
                    verifyHash = virtualPc.VirtualPcName + virtualPc.Service + virtualPc.OperatingSystem +
                                 virtualPc.CpuCores + virtualPc.RamSize + virtualPc.DiskSize + virtualPc.Backupping +
                                 virtualPc.Administration + virtualPc.IpAddress + virtualPc.Fqdn + virtualPc.Notes + customers;
                    break;
                case(null, null, not null):
                    verifyHash = account.VirtualPcId + account.Username + account.Password +
                                 account.BackupPassword + account.Admin;
                    break;
                default:
                    return verifyHash;
            }

            verifyHash = verifyHash.Replace(" ", "").ToLower();

            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(verifyHash));
            StringBuilder builder = new StringBuilder();
            foreach (var t in bytes)
            {
                builder.Append(t.ToString("x2"));
            }
            return builder.ToString();
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error generating verify hash: {ex.Message}", "OK");
            throw;
        }
    }
}