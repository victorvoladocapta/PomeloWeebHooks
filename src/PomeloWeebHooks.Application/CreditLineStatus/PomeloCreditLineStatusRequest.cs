using System.Text.Json.Serialization;

namespace PomeloWeebHooks.Application.CreditLineStatus;

public sealed record PomeloCreditLineStatusRequest(
    [property: JsonPropertyName("event_id")] string? EventId,
    [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey,
    [property: JsonPropertyName("data")] PomeloCreditLineStatusData? Data);

public sealed record PomeloCreditLineStatusData(
    [property: JsonPropertyName("credit_line_id")] string? CreditLineId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("reason")] string? Reason);
