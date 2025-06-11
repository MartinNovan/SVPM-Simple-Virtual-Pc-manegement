using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using SVPM.Models;
using SVPM.Repositories;

namespace SVPM.ViewModels;

public class AccountViewModel
{
    public static AccountViewModel Instance { get; } = new();
    private ObservableCollection<Account> Accounts { get; } = new();
    public ObservableCollection<Account> SortedAccounts { get; } = new();
    
    private AccountViewModel()
    {
        Accounts.CollectionChanged += Accounts_CollectionChanged;
    }

    private void Accounts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (Account old in e.OldItems)
                old.PropertyChanged -= Account_PropertyChanged;

        if (e.NewItems != null)
            foreach (Account @new in e.NewItems)
                @new.PropertyChanged += Account_PropertyChanged;

        SortAccounts();
    }

    private void Account_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SortAccounts();
    }
    private void SortAccounts(string searchText = "")
    {
        SortedAccounts.Clear();
        if (string.IsNullOrEmpty(searchText))
        {
            foreach (var account in Accounts.OrderBy(a => a.Username))
            {
                if(account.RecordState != RecordStates.Deleted) SortedAccounts.Add(account);
            }
            return;
        }
        
        searchText = searchText.ToLower();
        var filtered = Accounts
            .Where(a =>
                (a.Username != null && a.Username.ToLower().Contains(searchText)) ||
                (a.Password != null && a.Password.ToLower().Contains(searchText)) ||
                (a.BackupPassword != null && a.BackupPassword.ToLower().Contains(searchText)))
            .Where(a => a.RecordState != RecordStates.Deleted)
            .OrderBy(a => a.Username);

        foreach (var account in filtered)
        {
            SortedAccounts.Add(account);
        }

    }
    
    public void FilterAccounts(string searchText)
    { 
        SortAccounts(searchText);
    }
    
    public async Task LoadAccountsAsync()
    {
        var accounts = await AccountRepository.GetAllAccountsAsync();
        
        Accounts.Clear();
        foreach (var account in accounts)
        {
            Accounts.Add(account);
        }
    }
    
    public Task RemoveAccount(Account account)
    {
        account.RecordState = RecordStates.Deleted;
        if (account.OriginalRecordState != RecordStates.Loaded)
        {
            Accounts.Remove(account);
        }

        return Task.CompletedTask;
    }

    public Task SaveAccount(Account account)
    {
        var match = Accounts.FirstOrDefault(a => a.AccountId == account.AccountId);
        if (match != null)
        {
            Accounts.Remove(match);
            account.RecordState = RecordStates.Updated;
        }
        else
        {
            account.RecordState = RecordStates.Created;
        }
        Accounts.Add(account);
        return Task.CompletedTask;
    }
    
    public async Task UploadChanges()
    {
        foreach (var account in Accounts.Where(c => c.RecordState != RecordStates.Loaded).OrderBy(c => c.RecordState == RecordStates.Deleted ? 0 :
                     c.RecordState == RecordStates.Created ? 1 :
                     c.RecordState == RecordStates.Updated ? 2 : 3).ToList())
        {
            switch (account.RecordState)
            {
                case RecordStates.Created:
                    await AccountRepository.AddAccount(account);
                    account.RecordState = RecordStates.Loaded;
                    account.InitializeOriginalValues();
                    break;
                case RecordStates.Updated:
                    if (account.OriginalRecordState != RecordStates.Loaded) { await AccountRepository.AddAccount(account); return; }
                    await AccountRepository.UpdateAccount(account);
                    account.RecordState = RecordStates.Loaded;
                    account.InitializeOriginalValues();
                    break;
                case RecordStates.Deleted:
                    if (account.OriginalRecordState != RecordStates.Loaded){ Accounts.Remove(account); return;}
                    await AccountRepository.DeleteAccount(account);
                    Accounts.Remove(account);
                    break;
            }
        }
    }
}