using System.Text.Json;
using System.Text.Json.Nodes;
using PomeloWeebHooks.Application.Common;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Chargebacks;

public interface IPomeloChargebackStore
{
    Task<bool> TryAddAsync(PomeloChargebackEvent entity, CancellationToken cancellationToken);
}

public sealed class PomeloChargebackService(IPomeloChargebackStore store)
{
    public async Task<IngestResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(rawJson); }
        catch (JsonException) { return IngestResult.Invalid("El cuerpo JSON no es válido."); }

        var eventId = root?["event_id"]?.GetValue<string>();
        if (!string.Equals(eventId, "chargeback_notification", StringComparison.OrdinalIgnoreCase))
            return IngestResult.Invalid("event_id no es soportado.");

        var idempotency = root?["idempotency_key"]?.GetValue<string>();
        var transactionId = root?["transaction_id"]?.GetValue<string>();
        var chargebackId = root?["id"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(idempotency))
            return IngestResult.Invalid("idempotency_key es requerido.");
        if (string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(chargebackId))
            return IngestResult.Invalid("id y transaction_id son requeridos.");

        var entity = new PomeloChargebackEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotency.Trim(),
            EventId = eventId!.Trim(),
            ChargebackId = chargebackId.Trim(),
            TransactionId = transactionId.Trim(),
            Status = root?["status"]?.GetValue<string>(),
            StatusTicket = root?["status_ticket"]?.GetValue<string>(),
            Amount = root?["amount"]?.ToString(),
            Currency = root?["currency"]?.GetValue<string>(),
            PayloadJson = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProductStatus = PomeloProductStatus.Pending,
        };

        var created = await store.TryAddAsync(entity, ct);
        return IngestResult.Accepted(!created);
    }
}
