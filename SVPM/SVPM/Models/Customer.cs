using System.ComponentModel;
using System.Runtime.CompilerServices;
using SVPM.Repositories;

namespace SVPM.Models;
public sealed class Customer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Guid CustomerId { get; init; }

    private string? _fullName;
    public string? FullName
    {
        get => _fullName;
        set
        {
            if (_fullName != value)
            {
                _fullName = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _customerTag;
    public string? CustomerTag
    {
        get => _customerTag;
        set
        {
            if (_customerTag != value)
            {
                _customerTag = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _email;
    public string? Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _phone;
    public string? Phone
    {
        get => _phone;
        set
        {
            if (_phone != value)
            {
                _phone = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set
        {
            if (_notes != value)
            {
                _notes = value;
                NotifyPropertyChanged();
            }
        }
    }
    private DateTime _updated;
    public DateTime Updated
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

    public string? OriginalCustomerTag { get; private set; }
    public string? OriginalVerifyHash { get; private set; }
    public RecordStates OriginalRecordState { get; private set; }
    public void InitializeOriginalValues()
    {
        OriginalCustomerTag = CustomerTag;
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
                    await CustomerRepository.AddCustomer(this);
                    break;
                case RecordStates.Deleted:
                    await CustomerRepository.DeleteCustomer(this);
                    break;
                case RecordStates.Updated:
                    await CustomerRepository.UpdateCustomer(this);
                    break;
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}