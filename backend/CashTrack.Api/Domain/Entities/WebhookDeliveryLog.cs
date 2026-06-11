namespace CashTrack.Api.Domain.Entities;

public sealed class WebhookDeliveryLog
{
    public string Id { get; set; } = string.Empty;
    public string WebhookEndpointId { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string? Error { get; set; }
    public DateTime TimestampUtc { get; set; }

    public WebhookEndpoint? WebhookEndpoint { get; set; }
    public Transaction? Transaction { get; set; }
}
