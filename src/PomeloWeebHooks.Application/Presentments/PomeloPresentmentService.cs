using System.Text.Json;
using System.Text.Json.Nodes;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Presentments;

public interface IPomeloPresentmentStore
{
    Task<bool> TryAddAsync(PomeloPresentmentEvent entity, CancellationToken cancellationToken);
}

public sealed class PomeloPresentmentService(IPomeloPresentmentStore store)
{
    public async Task<TransactionNotifications.IngestResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(rawJson); }
        catch (JsonException) { return TransactionNotifications.IngestResult.Invalid("El cuerpo JSON no es válido."); }

        var eventId = root?["event_id"]?.GetValue<string>();
        if (!string.Equals(eventId, "presentment-notification", StringComparison.OrdinalIgnoreCase))
            return TransactionNotifications.IngestResult.Invalid("event_id no es soportado.");

        var detail = root?["event_detail"];
        var idempotency = root?["idempotency_key"]?.GetValue<string>()
            ?? detail?["id"]?.GetValue<string>()
            ?? detail?["public_id"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(idempotency))
            return TransactionNotifications.IngestResult.Invalid("idempotency_key es requerido.");

        var amounts = detail?["amounts"]?["transaction_amount"];
        var entity = new PomeloPresentmentEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotency.Trim(),
            EventId = eventId!.Trim(),
            PresentmentId = detail?["id"]?.GetValue<string>() ?? detail?["public_id"]?.GetValue<string>(),
            Status = detail?["status"]?.GetValue<string>(),
            PomeloUserId = detail?["user_id"]?.GetValue<string>(),
            PomeloCardId = detail?["card_id"]?.GetValue<string>(),
            OriginalTransactionId = detail?["original_transaction_data"]?["transaction_id"]?.GetValue<string>(),
            AmountTotal = amounts?["amount"]?.GetValue<string>(),
            AmountCurrency = amounts?["currency"]?.GetValue<string>(),
            PayloadJson = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProductStatus = PomeloProductStatus.Pending,
        };

        var created = await store.TryAddAsync(entity, ct);
        return TransactionNotifications.IngestResult.Accepted(!created);
    }
}
