namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloTransactionNotificationEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Variant { get; set; } = "network"; // network | on_us
    public string? TransactionId { get; set; }
    public string? Status { get; set; }
    public string? StatusDetail { get; set; }
    public string? PomeloUserId { get; set; }
    public string? PomeloCardId { get; set; }
    public string? MerchantName { get; set; }
    public string? LocalAmountTotal { get; set; }
    public string? LocalAmountCurrency { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public PomeloProductStatus ProductStatus { get; set; } = PomeloProductStatus.Pending;
    public DateTimeOffset? ProductProcessedAt { get; set; }
    public string? ProductError { get; set; }
}
