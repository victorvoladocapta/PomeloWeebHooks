namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloShippingEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string ShipmentId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? StatusDetail { get; set; }
    public string? RequestStatus { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public PomeloProductStatus ProductStatus { get; set; } = PomeloProductStatus.Pending;
    public DateTimeOffset? ProductProcessedAt { get; set; }
    public string? ProductError { get; set; }
}
