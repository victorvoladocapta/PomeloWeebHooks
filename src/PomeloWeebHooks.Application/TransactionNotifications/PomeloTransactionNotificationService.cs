using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.TransactionNotifications;

public interface IPomeloTransactionNotificationStore
{
    Task<bool> TryAddAsync(PomeloTransactionNotificationEvent entity, CancellationToken cancellationToken);
}

public sealed class PomeloTransactionNotificationService(IPomeloTransactionNotificationStore store)
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "authorization-advice",
        "approved-authorization-advice",
    };

    public async Task<IngestResult> ProcessAsync(string rawJson, string variant, CancellationToken ct)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(rawJson); }
        catch (JsonException) { return IngestResult.Invalid("El cuerpo JSON no es válido."); }

        var eventId = root?["event_id"]?.GetValue<string>();
        var idempotency = root?["idempotency_key"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(eventId) || !Allowed.Contains(eventId))
            return IngestResult.Invalid("event_id no es soportado.");
        if (string.IsNullOrWhiteSpace(idempotency))
            return IngestResult.Invalid("idempotency_key es requerido.");

        var detail = root?["event_detail"];
        var tx = detail?["transaction"];
        var card = detail?["card"];
        var user = detail?["user"];
        var merchant = detail?["merchant"];
        var amount = detail?["amount"]?["local"];

        var entity = new PomeloTransactionNotificationEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotency.Trim(),
            EventId = eventId.Trim(),
            Variant = variant,
            TransactionId = tx?["id"]?.GetValue<string>(),
            Status = detail?["status"]?.GetValue<string>(),
            StatusDetail = detail?["status_detail"]?.GetValue<string>(),
            PomeloUserId = user?["id"]?.GetValue<string>(),
            PomeloCardId = card?["id"]?.GetValue<string>(),
            MerchantName = merchant?["name"]?.GetValue<string>(),
            LocalAmountTotal = amount?["total"]?.GetValue<string>(),
            LocalAmountCurrency = amount?["currency"]?.GetValue<string>(),
            PayloadJson = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProductStatus = PomeloProductStatus.Pending,
        };

        var created = await store.TryAddAsync(entity, ct);
        return IngestResult.Accepted(!created);
    }
}

public sealed record IngestResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static IngestResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static IngestResult Invalid(string error) => new(false, false, error);
}
