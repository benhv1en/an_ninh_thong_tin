namespace CashTrack.Api.Domain.Entities;

public sealed class ClientStorageEnvelope
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public int Version { get; set; }
    public string Algorithm { get; set; } = string.Empty;
    public string? KeyFingerprint { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }

    public UserAccount? User { get; set; }
}
