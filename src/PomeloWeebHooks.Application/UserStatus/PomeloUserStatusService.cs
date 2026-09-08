using System.Text.Json;
using System.Text.Json.Nodes;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.UserStatus;

public interface IPomeloUserStatusStore
{
    Task<bool> TryAddAsync(PomeloUserStatusEvent entity, CancellationToken cancellationToken);
}

public sealed class PomeloUserStatusService(IPomeloUserStatusStore store)
{
    public async Task<TransactionNotifications.IngestResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(rawJson); }
        catch (JsonException) { return TransactionNotifications.IngestResult.Invalid("El cuerpo JSON no es válido."); }

        var eventId = root?["event_id"]?.GetValue<string>();
        var idempotency = root?["idempotency_key"]?.GetValue<string>();
        var userId = root?["user_id"]?.GetValue<string>();
        if (!string.Equals(eventId, "users_status_changed", StringComparison.OrdinalIgnoreCase))
            return TransactionNotifications.IngestResult.Invalid("event_id no es soportado.");
        if (string.IsNullOrWhiteSpace(idempotency))
            return TransactionNotifications.IngestResult.Invalid("idempotency_key es requerido.");
        if (string.IsNullOrWhiteSpace(userId))
            return TransactionNotifications.IngestResult.Invalid("user_id es requerido.");

        var entity = new PomeloUserStatusEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotency.Trim(),
            EventId = eventId!.Trim(),
            PomeloUserId = userId.Trim(),
            UserStatus = root?["user_status"]?.GetValue<string>(),
            UserStatusReason = root?["user_status_reason"]?.GetValue<string>(),
            PayloadJson = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProductStatus = PomeloProductStatus.Pending,
        };

        var created = await store.TryAddAsync(entity, ct);
        return TransactionNotifications.IngestResult.Accepted(!created);
    }
}
