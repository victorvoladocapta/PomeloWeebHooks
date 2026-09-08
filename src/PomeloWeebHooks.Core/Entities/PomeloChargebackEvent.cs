namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloChargebackEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string ChargebackId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? StatusTicket { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public PomeloProductStatus ProductStatus { get; set; } = PomeloProductStatus.Pending;
    public DateTimeOffset? ProductProcessedAt { get; set; }
    public string? ProductError { get; set; }
}
