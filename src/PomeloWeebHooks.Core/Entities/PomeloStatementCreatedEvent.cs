namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloStatementCreatedEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string StatementId { get; set; } = string.Empty;
    public string CreditLineId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
    public PomeloProductStatus ProductStatus { get; set; } = PomeloProductStatus.Pending;
    public DateTimeOffset? ProductProcessedAt { get; set; }
    public string? ProductError { get; set; }
}
