using CashTrack.Api.Domain;

namespace CashTrack.Api.Domain.Entities;

public sealed class ImportRow
{
    public string Id { get; set; } = string.Empty;
    public string ImportBatchId { get; set; } = string.Empty;
    public int RowNumber { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public ImportRowStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TransactionId { get; set; }

    public ImportBatch? ImportBatch { get; set; }
    public Transaction? Transaction { get; set; }
}
