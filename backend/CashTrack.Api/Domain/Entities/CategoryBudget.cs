using CashTrack.Api.Domain;

namespace CashTrack.Api.Domain.Entities;

public sealed class CategoryBudget
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public long LimitMinorUnits { get; set; }
    public BudgetPeriod Period { get; set; } = BudgetPeriod.monthly;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public UserAccount? User { get; set; }
    public TransactionCategory? Category { get; set; }
}
