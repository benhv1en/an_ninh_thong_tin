namespace CashTrack.Api.Domain.Entities;

public sealed class WebhookHeader
{
    public string Id { get; set; } = string.Empty;
    public string WebhookEndpointId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ValueEncrypted { get; set; } = string.Empty;

    public WebhookEndpoint? WebhookEndpoint { get; set; }
}
