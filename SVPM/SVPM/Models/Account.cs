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

    public Guid AccountID { get; set; }

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
                NotifyPropertyChanged(nameof(VirtualPcName));
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

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set
        {
            if (_isAdmin != value)
            {
                _isAdmin = value;
                NotifyPropertyChanged();
            }
        }
    }

    private DateTime? _lastUpdated;
    public DateTime? LastUpdated
    {
        get => _lastUpdated;
        set
        {
            if (_lastUpdated != value)
            {
                _lastUpdated = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _originalPassword;
    public string? OriginalPassword
    {
        get => _originalPassword;
        set
        {
            if (_originalPassword != value)
            {
                _originalPassword = value;
                NotifyPropertyChanged();
            }
        }
    }

    public string? VirtualPcName => AssociatedVirtualPc?.VirtualPcName;

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
                    await AccountRepository.DeleteAccount(AccountID);
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
