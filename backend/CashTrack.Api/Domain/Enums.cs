namespace CashTrack.Api.Domain;

public enum TransactionType
{
    income,
    expense
}

public enum TransactionSource
{
    notification,
    manual,
    api
}

public enum BudgetPeriod
{
    daily,
    weekly,
    monthly
}

public enum NotificationPermissionStatus
{
    authorized,
    denied,
    unknown
}

public enum FilterPeriod
{
    day,
    week,
    month,
    year,
    custom
}

public enum ImportFileType
{
    json,
    xlsx,
    xls
}

public enum ImportStatus
{
    running,
    completed,
    failed
}

public enum ImportRowStatus
{
    imported,
    updated,
    skipped,
    failed
}

public static class TransactionCategoryIds
{
    public const string Food = "food";
    public const string Shopping = "shopping";
    public const string Transport = "transport";
    public const string Entertainment = "entertainment";
    public const string Bills = "bills";
    public const string Health = "health";
    public const string Education = "education";
    public const string Salary = "salary";
    public const string Transfer = "transfer";
    public const string Investment = "investment";
    public const string Gift = "gift";
    public const string Other = "other";
}

public static class WebhookEvents
{
    public const string TransactionCreated = "transaction.created";
    public const string TransactionUpdated = "transaction.updated";
    public const string TransactionDeleted = "transaction.deleted";
    public const string BudgetExceeded = "budget.exceeded";
    public const string BudgetWarning = "budget.warning";
    public const string DailySummary = "daily.summary";
    public const string WeeklySummary = "weekly.summary";
    public const string MonthlySummary = "monthly.summary";
    public const string NotificationReceived = "notification.received";
}
