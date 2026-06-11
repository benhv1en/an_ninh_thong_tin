namespace CashTrack.Api.Domain.Entities;

public sealed class WebhookEventSubscription
{
    public string Id { get; set; } = string.Empty;
    public string WebhookEndpointId { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;

    public WebhookEndpoint? WebhookEndpoint { get; set; }
}
