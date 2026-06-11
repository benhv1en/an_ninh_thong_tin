using CashTrack.Api.Domain;

namespace CashTrack.Api.Domain.Entities;

public sealed class Transaction
{
    public string Id { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long AmountMinorUnits { get; set; }
    public TransactionType Type { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Merchant { get; set; }
    public string? BankCode { get; set; }
    public string? BankAccount { get; set; }
    public TransactionSource Source { get; set; } = TransactionSource.manual;
    public string? RawNotification { get; set; }
    public string? RawNotificationHash { get; set; }
    public DateTime TransactionDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? ImportBatchId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public UserAccount? User { get; set; }
    public TransactionCategory? Category { get; set; }
    public BankApp? BankApp { get; set; }
    public ImportBatch? ImportBatch { get; set; }
    public BankNotification? SourceNotification { get; set; }
    public ICollection<WebhookDeliveryLog> WebhookDeliveryLogs { get; set; } = new List<WebhookDeliveryLog>();
}
