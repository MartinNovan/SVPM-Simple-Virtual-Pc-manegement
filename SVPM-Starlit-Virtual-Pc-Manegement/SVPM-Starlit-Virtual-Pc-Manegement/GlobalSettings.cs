namespace SVPM_Starlit_Virtual_Pc_Manegement;

public static class GlobalSettings
{
    public static string? ConnectionString { get; set; }
    public const string CustomerTable = "Customers";
    public const string VirtualPcTable = "VirtualPCs";
    public const string AccountTable = "Accounts";
    public const string CustomersVirtualPcTable = "CustomersVirtualPCs";
    public static readonly string ConnectionListPath = FileHelpers.GetUserProfilePath() + "sql_connections.json";
}