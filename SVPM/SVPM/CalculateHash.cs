using System.Security.Cryptography;
using System.Text;
using SVPM.Models;

namespace SVPM;

public static class CalculateHash
{
    public static string CalculateVerifyHash(Customer? customer = null, VirtualPc? virtualPc = null, Account? account = null)
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
                    verifyHash = customer.FullName + customer.CustomerTag + customer.Email + customer.Phone + customer.Notes;
                    break;
                case (null, not null, null):
                    var customersId = "";
                    if (virtualPc.OwningCustomers?.Count > 0)
                    {
                        customersId = string.Join(", ", virtualPc.OwningCustomers.Select(c => c.CustomerId));
                    }
                    verifyHash = virtualPc.VirtualPcName + virtualPc.Service + virtualPc.OperatingSystem +
                                 virtualPc.CpuCores + virtualPc.RamSize + virtualPc.DiskSize + virtualPc.Backupping +
                                 virtualPc.Administration + virtualPc.IpAddress + virtualPc.Fqdn + virtualPc.Notes + customersId;
                    break;
                case(null, null, not null):
                    verifyHash = account.AssociatedVirtualPc!.VirtualPcId + account.Username + account.Password +
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
            Application.Current!.Windows[0].Page!.DisplayAlert("Error", $"Error generating verify hash: {ex.Message}", "OK");
            throw;
        }
    }
}