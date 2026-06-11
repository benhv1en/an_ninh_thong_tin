using CashTrack.Api.Domain;

namespace CashTrack.Api.Domain.Entities;

public sealed class UserSettings
{
    public string UserId { get; set; } = string.Empty;
    public string Language { get; set; } = "vi";
    public string Currency { get; set; } = "VND";
    public bool NotificationEnabled { get; set; }
    public NotificationPermissionStatus NotificationPermission { get; set; } = NotificationPermissionStatus.unknown;
    public long MonthlyBudgetMinorUnits { get; set; } = 10_000_000;
    public bool IsCustomBudget { get; set; }
    public int BudgetAlertThreshold { get; set; } = 80;
    public bool ShowCents { get; set; }
    public bool CompactNumbers { get; set; } = true;
    public string? GeminiApiKeyEncrypted { get; set; }
    public bool UseAICategorization { get; set; }
    public bool UseAIReports { get; set; }
    public bool WebhooksEnabled { get; set; }
    public FilterPeriod CurrentFilterPeriod { get; set; } = FilterPeriod.month;
    public DateTime? FilterStartDateUtc { get; set; }
    public DateTime? FilterEndDateUtc { get; set; }
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public UserAccount? User { get; set; }
}
