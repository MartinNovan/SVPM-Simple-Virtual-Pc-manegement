using SVPM.Repositories;

namespace SVPM.Models;

public class Customer
{
    public Guid CustomerID { get; init; }
    public string? FullName { get; set; }
    public string? CustomerTag { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public RecordStates RecordState { get; set; }

    public string? OriginalFullName { get; private set; }
    public string? OriginalCustomerTag { get; private set; }
    public string? OriginalEmail { get; private set; }
    public string? OriginalPhone { get; private set; }
    public string? OriginalNotes { get; private set; }
    public bool inDatabase { get; set; }

    public void InitializeOriginalValues()
    {
        if (RecordState != RecordStates.Loaded) return;
        OriginalFullName = FullName;
        OriginalCustomerTag = CustomerTag;
        OriginalEmail = Email;
        OriginalPhone = Phone;
        OriginalNotes = Notes;
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