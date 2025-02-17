using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SVPM.Models;

public class SqlConnection : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string? _name = String.Empty;
    public string? Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _serverAddress = String.Empty;
    public string? ServerAddress
    {
        get => _serverAddress;
        set
        {
            if (_serverAddress != value)
            {
                _serverAddress = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _databaseName = String.Empty;
    public string? DatabaseName
    {
        get => _databaseName;
        set
        {
            if (_databaseName != value)
            {
                _databaseName = value;
                NotifyPropertyChanged();
            }
        }
    }

    private bool _useWindowsAuth;
    public bool UseWindowsAuth
    {
        get => _useWindowsAuth;
        set
        {
            if (_useWindowsAuth != value)
            {
                _useWindowsAuth = value;
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

    private bool _useCertificate;
    public bool UseCertificate
    {
        get => _useCertificate;
        set
        {
            if (_useCertificate != value)
            {
                _useCertificate = value;
                NotifyPropertyChanged();
            }
        }
    }

    private string? _certificatePath;
    public string? CertificatePath
    {
        get => _certificatePath;
        set
        {
            if (_certificatePath != value)
            {
                _certificatePath = value;
                NotifyPropertyChanged();
            }
        }
    }
}
