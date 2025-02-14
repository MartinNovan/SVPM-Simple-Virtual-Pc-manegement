namespace SVPM.Models;

public class SqlConnection
{
    public string? Name { get; set; } = String.Empty;
    public string? ServerAddress { get; set; } = String.Empty;
    public string? DatabaseName { get; set; } = String.Empty;
    public bool UseWindowsAuth { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseCertificate { get; set; }
    public string? CertificatePath { get; set; }
}