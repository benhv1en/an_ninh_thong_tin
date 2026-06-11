namespace CashTrack.Api.Domain.Entities;

public sealed class UserSelectedBankApp
{
    public string UserId { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public UserAccount? User { get; set; }
    public BankApp? BankApp { get; set; }
}
