using System.Text.Json;
using System.Text.Json.Nodes;
using PomeloWeebHooks.Application.Common;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Shipping;

public interface IPomeloShippingStore
{
    Task<bool> TryAddAsync(PomeloShippingEvent entity, CancellationToken cancellationToken);
}

public sealed class PomeloShippingService(IPomeloShippingStore store)
{
    public async Task<IngestResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(rawJson); }
        catch (JsonException) { return IngestResult.Invalid("El cuerpo JSON no es válido."); }

        var shipmentId = root?["shipment_id"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(shipmentId))
            return IngestResult.Invalid("shipment_id es requerido.");

        var eventId = root?["event_id"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(eventId))
            eventId = "shipment-status-changed";

        var status = root?["status"]?.GetValue<string>();
        var updatedAt = root?["updated_at"]?.GetValue<string>();
        var idempotency = root?["idempotency_key"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(idempotency))
            idempotency = string.Join(':', new[] { shipmentId, status, updatedAt }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var entity = new PomeloShippingEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotency.Trim(),
            EventId = eventId.Trim(),
            ShipmentId = shipmentId.Trim(),
            Status = status,
            StatusDetail = root?["status_detail"]?.GetValue<string>(),
            RequestStatus = root?["request_status"]?.GetValue<string>(),
            PayloadJson = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProductStatus = PomeloProductStatus.Pending,
        };

        var created = await store.TryAddAsync(entity, ct);
        return IngestResult.Accepted(!created);
    }
}
