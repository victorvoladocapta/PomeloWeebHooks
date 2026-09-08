using System.Text.Json;
using System.Text.Json.Nodes;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.InterestCharges;

public interface IPomeloInterestChargeStore
{
    Task<bool> TryAddAsync(PomeloInterestChargeEvent entity, CancellationToken cancellationToken);
}

public sealed class PomeloInterestChargeService(IPomeloInterestChargeStore store)
{
    public async Task<TransactionNotifications.IngestResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(rawJson); }
        catch (JsonException) { return TransactionNotifications.IngestResult.Invalid("El cuerpo JSON no es válido."); }

        var eventId = root?["event_id"]?.GetValue<string>();
        var idempotency = root?["idempotency_key"]?.GetValue<string>();
        var data = root?["data"];
        if (!string.Equals(eventId, "interest_created", StringComparison.OrdinalIgnoreCase))
            return TransactionNotifications.IngestResult.Invalid("event_id no es soportado.");
        if (string.IsNullOrWhiteSpace(idempotency))
            return TransactionNotifications.IngestResult.Invalid("idempotency_key es requerido.");
        var interestId = data?["id"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(interestId))
            return TransactionNotifications.IngestResult.Invalid("data.id es requerido.");

        var amount = data?["amount"];
        var entity = new PomeloInterestChargeEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotency.Trim(),
            EventId = eventId!.Trim(),
            InterestId = interestId.Trim(),
            PomeloUserId = data?["user_id"]?.GetValue<string>(),
            CreditLineId = data?["credit_line_id"]?.GetValue<string>(),
            DebtId = data?["debt_id"]?.GetValue<string>(),
            Origin = data?["origin"]?.GetValue<string>(),
            Type = data?["type"]?.GetValue<string>(),
            AmountTotal = amount?["total"]?.GetValue<string>(),
            AmountCurrency = amount?["currency"]?.GetValue<string>(),
            EffectiveAt = data?["effective_at"]?.GetValue<string>(),
            PayloadJson = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProductStatus = PomeloProductStatus.Pending,
        };

        var created = await store.TryAddAsync(entity, ct);
        return TransactionNotifications.IngestResult.Accepted(!created);
    }
}
