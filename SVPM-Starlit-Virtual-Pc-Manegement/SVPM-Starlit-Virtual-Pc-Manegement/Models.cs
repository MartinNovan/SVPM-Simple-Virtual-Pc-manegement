namespace SVPM_Starlit_Virtual_Pc_Manegement;

public static class Models
{
    //TODO: Dodělat ověřování toho že se v databázi nezměnila hodnota zatímco uživatel pracoval s lokalní instancí (udělat kopii každé vlastnosti při načtení z databáze a porovnávat je při ukládání změn)
    public enum RecordStates
    {
        Loaded,
        Created,
        Deleted,
        Updated
    }
    public class Customer
    {
        public Guid CustomerID { get; init; }
        public string? FullName { get; init; }
        public string? CustomerTag { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? Notes { get; set; }
        public RecordStates RecordState { get; set; }
        public async void SaveChanges()
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
                        await CustomerRepository.DeleteCustomer(CustomerID);
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

    public class VirtualPC
    {
        public Guid VirtualPcID { get; init; }
        public string? VirtualPcName { get; init; }
        public string? ServiceName { get; set; }
        public string? OperatingSystem { get; set; }
        public int CPU_Cores { get; set; }
        public int RAM_Size_GB { get; set; }
        public int Disk_Size_GB { get; set; }
        public bool Backupping { get; set; }
        public bool Administration { get; set; }
        public string? IP_Address { get; set; }
        public string? FQDN { get; set; }
        public string? Notes { get; set; }
        public List<Customer>? OwningCustomers { get; set; }
        public string OwningCustomersNames => OwningCustomers != null && OwningCustomers.Count > 0 ? string.Join(", ", OwningCustomers.Select(c => c.FullName)) : "No customers";
        public RecordStates RecordState { get; set; }
        public async void SaveChanges()
        {
            try
            {
                switch (RecordState)
                {
                    case RecordStates.Loaded:
                        break;
                    case RecordStates.Created:
                        await VirtualPcRepository.AddVirtualPc(this);
                        break;
                    case RecordStates.Deleted:
                        await VirtualPcRepository.DeleteVirtualPc(VirtualPcID);
                        break;
                    case RecordStates.Updated:
                        await VirtualPcRepository.UpdateVirtualPc(this);
                        break;
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    public class Account
    {
        public Guid AccountID { get; set; }
        public VirtualPC? AssociatedVirtualPc { get; set; }
        public string? Username { get; init; }
        public string? Password { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime LastUpdated { get; set; }
        public string OriginalPassword { get; set; }
        public string? VirtualPcName => AssociatedVirtualPc?.VirtualPcName;
        public RecordStates RecordState { get; set; }
        public async void SaveChanges()
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


    public class Mapping
    {
        public Guid MappingID { get; set; }
        public Guid VirtualPcID { get; set; }
        public Guid CustomerID { get; set; }
        public RecordStates RecordState { get; set; }
        public async void SaveChanges()
        {
            try
            {
                switch (RecordState)
                {
                    case RecordStates.Loaded:
                        break;
                    case RecordStates.Created:
                        await CustomersVirtualPCsRepository.AddMappingAsync(this);
                        break;
                    case RecordStates.Deleted:
                        await CustomersVirtualPCsRepository.DeleteMappingAsync(CustomerID, VirtualPcID);
                        break;
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    public class SqlConnections
    {
        public string? Name { get; init; }
        public string? ServerAddress { get; set; }
        public string? DatabaseName { get; set; }
        public bool UseWindowsAuth { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool UseCertificate { get; set; }
        public string? CertificatePath { get; set; }
    }
}
