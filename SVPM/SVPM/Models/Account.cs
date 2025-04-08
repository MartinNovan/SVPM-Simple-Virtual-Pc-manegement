using System.ComponentModel;
using System.Runtime.CompilerServices;
using SVPM.Repositories;

namespace SVPM.Models;
public class Account : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public Guid AccountId { get; set; }
    private VirtualPc? _associatedVirtualPc;
    public VirtualPc? AssociatedVirtualPc
    {
        get => _associatedVirtualPc;
        set
        {
            if (_associatedVirtualPc != value)
            {
                _associatedVirtualPc = value;
                NotifyPropertyChanged();
            }
        }
    }
    private string? _username;
    public string? Username
    {
        get => _username;
        set
        {
            if (_username != value)
            {
                _username = value;
                NotifyPropertyChanged();
            }
        }
    }
    private string? _password;
    public string? Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                NotifyPropertyChanged();
            }
        }
    }
    private string? _backupPassword;
    public string? BackupPassword
    {
        get => _backupPassword;
        set
        {
            if (_backupPassword != value)
            {
                _backupPassword = value;
                NotifyPropertyChanged();
            }
        }
    }
    private bool _admin;
    public bool Admin
    {
        get => _admin;
        set
        {
            if (_admin != value)
            {
                _admin = value;
                NotifyPropertyChanged();
            }
        }
    }

    private DateTime? _updated;
    public DateTime? Updated
    {
        get => _updated;
        set
        {
            if (_updated != value)
            {
                _updated = value;
                NotifyPropertyChanged();
            }
        }
    }
    public string? VerifyHash { get; set; }
    
    private RecordStates _recordState;
    public RecordStates RecordState
    {
        get => _recordState;
        set
        {
            if (_recordState != value)
            {
                _recordState = value;
                NotifyPropertyChanged();
            }
        }
    }
    public string? OriginalVerifyHash { get; private set; }
    public RecordStates OriginalRecordState { get; private set; }
    public void InitializeOriginalValues()
    {
        OriginalVerifyHash = VerifyHash;
        OriginalRecordState = RecordState;
    }
    public async Task SaveChanges()
    {
        try
        {
            switch (RecordState)
            {
                case RecordStates.Loaded:
                    break;
                case RecordStates.Created:
                    await AccountRepository.AddAccount(this);
                    break;
                case RecordStates.Deleted:
                    await AccountRepository.DeleteAccount(this);
                    break;
                case RecordStates.Updated:
                    await AccountRepository.UpdateAccount(this);
                    break;
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
