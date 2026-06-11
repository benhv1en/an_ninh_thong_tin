namespace CashTrack.Api.Domain.Entities;

public sealed class WebhookEndpoint
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string? SecretEncrypted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastTriggeredAtUtc { get; set; }
    public int FailCount { get; set; }

    public UserAccount? User { get; set; }
    public ICollection<WebhookEventSubscription> EventSubscriptions { get; set; } = new List<WebhookEventSubscription>();
    public ICollection<WebhookHeader> Headers { get; set; } = new List<WebhookHeader>();
    public ICollection<WebhookDeliveryLog> DeliveryLogs { get; set; } = new List<WebhookDeliveryLog>();
}
