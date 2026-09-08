namespace PomeloWeebHooks.Core.Entities;

public sealed class PomeloInterestChargeEvent
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string InterestId { get; set; } = string.Empty;
    public string? PomeloUserId { get; set; }
    public string? CreditLineId { get; set; }
    public string? DebtId { get; set; }
    public string? Origin { get; set; }
    public string? Type { get; set; }
    public string? AmountTotal { get; set; }
    public string? AmountCurrency { get; set; }
    public string? EffectiveAt { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset ReceivedAt { get; set; }
    public PomeloProductStatus ProductStatus { get; set; } = PomeloProductStatus.Pending;
    public DateTimeOffset? ProductProcessedAt { get; set; }
    public string? ProductError { get; set; }
}
