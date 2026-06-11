namespace CashTrack.Api.Domain.Entities;

public sealed class BankApp
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Logo { get; set; }

    public ICollection<UserSelectedBankApp> SelectedByUsers { get; set; } = new List<UserSelectedBankApp>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<BankNotification> Notifications { get; set; } = new List<BankNotification>();
}
