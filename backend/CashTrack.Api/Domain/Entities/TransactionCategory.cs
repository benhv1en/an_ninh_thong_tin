namespace CashTrack.Api.Domain.Entities;

public sealed class TransactionCategory
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string LabelVi { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string GradientStart { get; set; } = string.Empty;
    public string GradientEnd { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<CategoryBudget> CategoryBudgets { get; set; } = new List<CategoryBudget>();
}
