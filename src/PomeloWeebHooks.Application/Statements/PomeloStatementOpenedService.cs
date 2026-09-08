using System.Text.Json;
using System.Text.Json.Nodes;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Statements;

public interface IPomeloStatementOpenedStore
{
    Task<bool> TryAddAsync(PomeloStatementOpenedEvent entity, CancellationToken cancellationToken);
}

public sealed class PomeloStatementOpenedService(IPomeloStatementOpenedStore store)
{
    public async Task<TransactionNotifications.IngestResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(rawJson); }
        catch (JsonException) { return TransactionNotifications.IngestResult.Invalid("El cuerpo JSON no es válido."); }

        var eventId = root?["event_id"]?.GetValue<string>();
        var idempotency = root?["idempotency_key"]?.GetValue<string>();
        var data = root?["data"];
        if (!string.Equals(eventId, "statement_opened", StringComparison.OrdinalIgnoreCase))
            return TransactionNotifications.IngestResult.Invalid("event_id no es soportado.");
        if (string.IsNullOrWhiteSpace(idempotency))
            return TransactionNotifications.IngestResult.Invalid("idempotency_key es requerido.");
        var statementId = data?["id"]?.GetValue<string>();
        var creditLineId = data?["credit_line_id"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(statementId) || string.IsNullOrWhiteSpace(creditLineId))
            return TransactionNotifications.IngestResult.Invalid("data.id y data.credit_line_id son requeridos.");

        var entity = new PomeloStatementOpenedEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotency.Trim(),
            EventId = eventId!.Trim(),
            StatementId = statementId.Trim(),
            CreditLineId = creditLineId.Trim(),
            StartDate = data?["start_date"]?.GetValue<string>(),
            ClosingDate = data?["closing_date"]?.GetValue<string>(),
            PayloadJson = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProductStatus = PomeloProductStatus.Pending,
        };

        var created = await store.TryAddAsync(entity, ct);
        return TransactionNotifications.IngestResult.Accepted(!created);
    }
}
