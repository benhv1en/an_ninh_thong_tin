using CashTrack.Api.Domain;

namespace CashTrack.Api.Domain.Entities;

public sealed class ImportBatch
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public ImportFileType FileType { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.running;
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public string? ErrorMessage { get; set; }

    public UserAccount? User { get; set; }
    public ICollection<ImportRow> Rows { get; set; } = new List<ImportRow>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
