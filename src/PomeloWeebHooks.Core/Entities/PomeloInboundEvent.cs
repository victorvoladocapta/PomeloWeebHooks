namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloInboundEvent
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? RelatedResourceId { get; set; }
    public string? PomeloUserId { get; set; }
    public string? Status { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
