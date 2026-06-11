using System.Text.Json;

namespace CashTrack.Api.Contracts;

public sealed record TransactionListResponse(IReadOnlyList<TransactionDto> Items, string? NextCursor);

public sealed record TransactionDto(
    string Id,
    long Amount,
    string Type,
    string Category,
    string Description,
    string? Merchant,
    string? BankAccount,
    string Source,
    string? RawNotification,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed class CreateTransactionRequest
{
    public long? Amount { get; init; }
    public string? Type { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public string? Merchant { get; init; }
    public string? BankAccount { get; init; }
    public string? Source { get; init; }
    public string? RawNotification { get; init; }
    public string? ExternalId { get; init; }
    public DateTime? CreatedAt { get; init; }
}

public sealed class UpdateTransactionRequest
{
    public long? Amount { get; init; }
    public string? Type { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public string? Merchant { get; init; }
    public string? BankAccount { get; init; }
}

public sealed record CategoryDto(
    string Id,
    string Label,
    string LabelVi,
    string Icon,
    string Color,
    IReadOnlyList<string> Gradient);

public sealed record BankInfoDto(
    string Code,
    string Name,
    string ShortName,
    string PackageName,
    string Color,
    string? Logo);

public sealed class BankNotificationDto
{
    public string? App { get; init; }
    public string? Title { get; init; }
    public string? Text { get; init; }
    public JsonElement? Time { get; init; }
    public Dictionary<string, object?>? Extra { get; init; }
    public bool? Processed { get; init; }
}

public sealed class ParseNotificationRequest
{
    public BankNotificationDto? Notification { get; init; }
    public IReadOnlyList<string>? SelectedBankApps { get; init; }
    public bool? UseAi { get; init; }
}

public sealed record ParsedTransactionDto(
    long Amount,
    string Type,
    string? Merchant,
    string? Description,
    string? BankCode,
    string? AccountNumber,
    DateTime Time,
    string RawText);

public sealed record ParseNotificationResponse(
    bool IsBankingNotification,
    bool IsAdvertisement,
    ParsedTransactionDto? Parsed,
    string? SuggestedCategory,
    string? DuplicateKey,
    string? Reason);

public sealed record WebhookConfigDto(
    string Id,
    string Name,
    string Url,
    bool Enabled,
    IReadOnlyList<string> Events,
    IReadOnlyDictionary<string, string>? Headers,
    DateTime CreatedAt,
    DateTime? LastTriggeredAt,
    int FailCount,
    bool HasSecret);

public sealed class CreateWebhookRequest
{
    public string? Name { get; init; }
    public string? Url { get; init; }
    public bool? Enabled { get; init; }
    public string? Secret { get; init; }
    public IReadOnlyList<string>? Events { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}

public sealed class UpdateWebhookRequest
{
    public string? Name { get; init; }
    public string? Url { get; init; }
    public bool? Enabled { get; init; }
    public string? Secret { get; init; }
    public IReadOnlyList<string>? Events { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}

public sealed record WebhookPayloadDto(string Event, DateTime Timestamp, object? Data);

public sealed record WebhookDeliveryResultDto(
    string WebhookId,
    bool Success,
    int? StatusCode,
    string? Error,
    DateTime Timestamp);

public sealed class TestWebhookRequest
{
    public object? Payload { get; init; }
}

public sealed class SendWebhookRequest
{
    public string? Event { get; init; }
    public object? Data { get; init; }
    public IReadOnlyList<string>? WebhookIds { get; init; }
}

public sealed class BackupImportRequest
{
    public IReadOnlyList<ImportTransactionDto>? Transactions { get; init; }
}

public sealed class ImportTransactionDto
{
    public string? Id { get; init; }
    public long? Amount { get; init; }
    public string? Type { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public string? Merchant { get; init; }
    public string? BankAccount { get; init; }
    public string? Source { get; init; }
    public string? RawNotification { get; init; }
    public string? ExternalId { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record ImportErrorDto(int Row, string Field, string Message);

public sealed record ImportResultDto(
    bool Success,
    int Total,
    int Imported,
    int Updated,
    int Skipped,
    IReadOnlyList<ImportErrorDto> Errors);
