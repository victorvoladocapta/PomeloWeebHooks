namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloPresentmentEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string? PresentmentId { get; set; }
    public string? Status { get; set; }
    public string? PomeloUserId { get; set; }
    public string? PomeloCardId { get; set; }
    public string? OriginalTransactionId { get; set; }
    public string? AmountTotal { get; set; }
    public string? AmountCurrency { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public PomeloProductStatus ProductStatus { get; set; } = PomeloProductStatus.Pending;
    public DateTimeOffset? ProductProcessedAt { get; set; }
    public string? ProductError { get; set; }
}
