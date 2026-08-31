using System.Text.Json;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.CardEvents;

public sealed class PomeloCardEventService(IPomeloCardEventStore store)
{
    private static readonly HashSet<string> SupportedEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "CREATION", "ACTIVATION", "EMBOSSMENT", "BLOCK", "UNBLOCK", "DISABLEMENT",
    };

    public async Task<CardEventResult> ProcessAsync(string rawJson, CancellationToken cancellationToken)
    {
        PomeloCardEventRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<PomeloCardEventRequest>(rawJson);
        }
        catch (JsonException)
        {
            return CardEventResult.Invalid("El cuerpo JSON no es válido.");
        }

        var validationError = Validate(request);
        if (validationError is not null)
            return CardEventResult.Invalid(validationError);

        var entity = new PomeloCardEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = request!.IdempotencyKey!.Trim(),
            EventId = request.EventId!.Trim(),
            PomeloCardId = request.CardId!.Trim(),
            PomeloUserId = request.UserId!.Trim(),
            EventType = request.Event!.Trim().ToUpperInvariant(),
            CardType = request.CardType!.Trim().ToUpperInvariant(),
            RelatedCardId = NullIfWhiteSpace(request.RelatedCardId),
            PomeloUpdatedAt = request.UpdatedAt!.Value,
            PayloadJson = rawJson,
            Status = PomeloCardEventStatus.Processed,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessedAt = DateTimeOffset.UtcNow,
        };

        var outcome = await store.TryAddAsync(entity, cancellationToken);
        return CardEventResult.Accepted(outcome == StoreCardEventOutcome.Duplicate);
    }

    private static string? Validate(PomeloCardEventRequest? request)
    {
        if (request is null) return "El cuerpo es requerido.";
        if (string.IsNullOrWhiteSpace(request.EventId)) return "event_id es requerido.";
        if (string.IsNullOrWhiteSpace(request.CardId)) return "id es requerido.";
        if (request.UpdatedAt is null) return "updated_at es requerido.";
        if (string.IsNullOrWhiteSpace(request.UserId)) return "user_id es requerido.";
        if (string.IsNullOrWhiteSpace(request.Event)) return "event es requerido.";
        if (!SupportedEvents.Contains(request.Event)) return "event no es soportado.";
        if (string.IsNullOrWhiteSpace(request.CardType)) return "card_type es requerido.";
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) return "idempotency_key es requerido.";
        return null;
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
