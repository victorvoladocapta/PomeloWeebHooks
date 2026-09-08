namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloRevertedOperationEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string OperationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreditLineId { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public string? CardLastFour { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? MerchantId { get; set; }
    public string? MerchantName { get; set; }
    public string? InstallmentsQuantity { get; set; }
    public string RevertedDateTime { get; set; } = string.Empty;
    public string LocalAmountTotal { get; set; } = string.Empty;
    public string LocalAmountCurrency { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
