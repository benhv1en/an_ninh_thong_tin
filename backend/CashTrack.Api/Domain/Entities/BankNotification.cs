using CashTrack.Api.Domain;

namespace CashTrack.Api.Domain.Entities;

public sealed class BankNotification
{
    public string Id { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AppPackage { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime NotificationTimeUtc { get; set; }
    public string? ExtraJson { get; set; }
    public bool Processed { get; set; }
    public bool IsBankingNotification { get; set; }
    public bool IsAdvertisement { get; set; }
    public string RawTextHash { get; set; } = string.Empty;
    public long? ParsedAmountMinorUnits { get; set; }
    public TransactionType? ParsedType { get; set; }
    public string? ParsedMerchant { get; set; }
    public string? ParsedDescription { get; set; }
    public string? ParsedTransactionId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public UserAccount? User { get; set; }
    public BankApp? BankApp { get; set; }
    public Transaction? ParsedTransaction { get; set; }
}
