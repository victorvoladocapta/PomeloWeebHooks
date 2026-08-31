using System.Text.Json.Serialization;

namespace PomeloWeebHooks.Application.CardEvents;

public sealed record PomeloCardEventRequest(
    [property: JsonPropertyName("event_id")] string? EventId,
    [property: JsonPropertyName("id")] string? CardId,
    [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt,
    [property: JsonPropertyName("user_id")] string? UserId,
    [property: JsonPropertyName("event")] string? Event,
    [property: JsonPropertyName("card_type")] string? CardType,
    [property: JsonPropertyName("related_card_id")] string? RelatedCardId,
    [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey);
