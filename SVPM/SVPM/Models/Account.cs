using SVPM.Repositories;

namespace SVPM.Models;

public class Account
{
    public Guid AccountID { get; set; }
    public VirtualPC? AssociatedVirtualPc { get; set; }
    public string? Username { get; init; }
    public string? Password { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? OriginalPassword { get; set; }
    public string? VirtualPcName => AssociatedVirtualPc?.VirtualPcName;
    public RecordStates RecordState { get; set; }
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