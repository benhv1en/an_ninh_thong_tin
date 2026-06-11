namespace CashTrack.Api.Domain.Entities;

public sealed class UserAccount
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string DataKeySalt { get; set; } = string.Empty;
    public string EncryptedDataKey { get; set; } = string.Empty;
    public string PasswordAlgorithm { get; set; } = "bcrypt";
    public int BcryptRounds { get; set; }
    public string DataKeyAlgorithm { get; set; } = "AES-256-GCM";
    public string KeyDerivation { get; set; } = "PBKDF2-SHA256";
    public int KeyDerivationIterations { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public UserSettings? Settings { get; set; }
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<WebhookEndpoint> WebhookEndpoints { get; set; } = new List<WebhookEndpoint>();
    public ICollection<BankNotification> BankNotifications { get; set; } = new List<BankNotification>();
    public ICollection<CategoryBudget> CategoryBudgets { get; set; } = new List<CategoryBudget>();
    public ICollection<UserSelectedBankApp> SelectedBankApps { get; set; } = new List<UserSelectedBankApp>();
    public ICollection<ClientStorageEnvelope> ClientStorageEnvelopes { get; set; } = new List<ClientStorageEnvelope>();
    public ICollection<ImportBatch> ImportBatches { get; set; } = new List<ImportBatch>();
}
