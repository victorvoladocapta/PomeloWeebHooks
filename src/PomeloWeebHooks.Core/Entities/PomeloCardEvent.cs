namespace PomeloWeebHooks.Core.Entities;

public enum PomeloCardEventStatus
{
    Received = 1,
    Processed = 2,
}

public sealed class PomeloCardEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string PomeloCardId { get; set; } = string.Empty;
    public string PomeloUserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string? RelatedCardId { get; set; }
    public DateTimeOffset PomeloUpdatedAt { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public PomeloCardEventStatus Status { get; set; } = PomeloCardEventStatus.Received;
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
}
