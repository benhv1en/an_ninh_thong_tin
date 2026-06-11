namespace CashTrack.Api.Domain.Entities;

public sealed class UserSession
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime AuthenticatedAtUtc { get; set; }
    public string SecureChannelAlgorithm { get; set; } = string.Empty;
    public string SessionKeyAlgorithm { get; set; } = string.Empty;
    public string ServerKeyFingerprint { get; set; } = string.Empty;
    public string EncryptedSessionKey { get; set; } = string.Empty;
    public DateTime EstablishedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    public UserAccount? User { get; set; }
}
