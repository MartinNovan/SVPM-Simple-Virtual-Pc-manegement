namespace SVPM.Models;

public class CustomerLog : Customer
{
    public Guid AuditId { get; set; }
    public string? OperationType { get; set; }
    public string? ChangedBy { get; set; }
}

public class VirtualPcLog : VirtualPc
{
    public Guid AuditId { get; set; }
    public string? OperationType { get; set; }
    public string? ChangedBy { get; set; }
}

public class MappingLog : Mapping
{
    public Guid AuditId { get; set; }
    public string? OperationType { get; set; }
    public string? ChangedBy { get; set; }
    public string? CustomerFullName { get; set; }
    public string? VirtualPcName { get; set; }
}

public class AccountLog : Account
{
    public Guid AuditId { get; set; }
    public string? OperationType { get; set; }
    public string? ChangedBy { get; set; }
}