using SVPM.Models;
using SVPM.ViewModels;

namespace SVPM.Services;

public class AccountService
{
    public static AccountService Instance {get;} = new();
    public Task<VirtualPc?> GetAccountVirtualPC(Account account)
    {
        var virtualPc = VirtualPcViewModel.Instance.SortedVirtualPCs .FirstOrDefault(virtualpc => virtualpc.VirtualPcId == account.VirtualPcId && virtualpc.RecordState != RecordStates.Deleted);
        return Task.FromResult(virtualPc);
    }

    public async Task CreateAccount(VirtualPc virtualPc, string username, string? password = null, string? backupPassword = null, bool admin = false)
    {
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            VirtualPcId = virtualPc.VirtualPcId,
            Username = username,
            Password = password,
            BackupPassword = backupPassword,
            Admin = admin,
            RecordState = RecordStates.Created
        };
        
        account.VerifyHash = await CalculateHash.CalculateVerifyHash(null,null, account);
        account.InitializeOriginalValues();
        
        await AccountViewModel.Instance.SaveAccount(account);
    }
    
    public async Task UpdateAccount(Guid accountId, VirtualPc virtualPc, string username, string? password = null, string? backupPassword = null, bool admin = false)
    {
        var account = AccountViewModel.Instance.SortedAccounts.FirstOrDefault(a => a.AccountId == accountId);
        if (account != null)
        {
            account.VirtualPcId = virtualPc.VirtualPcId;
            account.Username = username;
            account.Password = password;
            account.BackupPassword = backupPassword;
            account.Admin = admin;
            account.RecordState = RecordStates.Updated;
            account.VerifyHash = await CalculateHash.CalculateVerifyHash(null,null, account);

            await AccountViewModel.Instance.SaveAccount(account);
        }
    }
}