using System.Text.Json;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.InboundEvents;

public enum PomeloInboundEventKind
{
    ProcessingTransaction,
    Presentment,
    StatementOpened,
    InterestOrCharge,
    UserStatusChanged,
}

public sealed class PomeloInboundEventService(IPomeloInboundEventStore store)
{
    public async Task<InboundEventResult> ProcessAsync(
        string rawJson,
        PomeloInboundEventKind kind,
        CancellationToken cancellationToken)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(rawJson);
        }
        catch (JsonException)
        {
            return InboundEventResult.Invalid("El cuerpo JSON no es válido.");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return InboundEventResult.Invalid("El cuerpo debe ser un objeto JSON.");

            var eventId = Read(root, "event_id");
            var idempotencyKey = Read(root, "idempotency_key");
            if (string.IsNullOrWhiteSpace(eventId))
                return InboundEventResult.Invalid("event_id es requerido.");
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return InboundEventResult.Invalid("idempotency_key es requerido.");

            var fields = Extract(root, kind);
            if (string.IsNullOrWhiteSpace(fields.ResourceId))
                return InboundEventResult.Invalid($"{fields.ResourceField} es requerido.");

            var now = DateTimeOffset.UtcNow;
            var entity = new PomeloInboundEvent
            {
                Id = Guid.NewGuid(),
                Kind = KindName(kind),
                EventId = eventId.Trim(),
                IdempotencyKey = idempotencyKey.Trim(),
                ResourceId = fields.ResourceId.Trim(),
                RelatedResourceId = fields.RelatedResourceId?.Trim(),
                PomeloUserId = fields.UserId?.Trim(),
                Status = fields.Status?.Trim(),
                PayloadJson = rawJson,
                ReceivedAt = now,
                ProcessedAt = now,
            };

            var outcome = await store.TryAddAsync(entity, cancellationToken);
            return InboundEventResult.Accepted(outcome == StoreInboundEventOutcome.Duplicate);
        }
    }

    private static ExtractedFields Extract(JsonElement root, PomeloInboundEventKind kind) => kind switch
    {
        PomeloInboundEventKind.ProcessingTransaction => new(
            First(root, "event_detail.transaction.id"), "event_detail.transaction.id",
            First(root, "event_detail.transaction.original_transaction_id"),
            First(root, "event_detail.user.id", "event_detail.instrument.id"),
            First(root, "event_detail.status")),
        PomeloInboundEventKind.Presentment => new(
            First(root, "event_detail.public_id", "event_detail.id"), "event_detail.public_id",
            First(root, "event_detail.original_transaction_data.transaction_id"),
            First(root, "event_detail.user_id"),
            First(root, "event_detail.status")),
        PomeloInboundEventKind.StatementOpened => new(
            First(root, "data.id"), "data.id",
            First(root, "data.credit_line_id"), null, null),
        PomeloInboundEventKind.InterestOrCharge => new(
            First(root, "data.id"), "data.id",
            First(root, "data.credit_line_id", "data.debt_id"),
            First(root, "data.user_id"),
            First(root, "data.type", "data.origin")),
        PomeloInboundEventKind.UserStatusChanged => new(
            First(root, "data.user_id", "data.id", "user.id", "user_id", "id"), "user id",
            null,
            First(root, "data.user_id", "data.id", "user.id", "user_id", "id"),
            First(root, "data.status", "user.status", "status")),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static string KindName(PomeloInboundEventKind kind) => kind switch
    {
        PomeloInboundEventKind.ProcessingTransaction => "PROCESSING_TRANSACTION",
        PomeloInboundEventKind.Presentment => "PRESENTMENT",
        PomeloInboundEventKind.StatementOpened => "STATEMENT_OPENED",
        PomeloInboundEventKind.InterestOrCharge => "INTEREST_OR_CHARGE",
        PomeloInboundEventKind.UserStatusChanged => "USER_STATUS_CHANGED",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static string? First(JsonElement root, params string[] paths)
    {
        foreach (var path in paths)
        {
            var value = Read(root, path);
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }
        return null;
    }

    private static string? Read(JsonElement root, string path)
    {
        var current = root;
        foreach (var segment in path.Split('.'))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
                return null;
        }
        return current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
    }

    private sealed record ExtractedFields(
        string? ResourceId,
        string ResourceField,
        string? RelatedResourceId,
        string? UserId,
        string? Status);
}

public sealed record InboundEventResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static InboundEventResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static InboundEventResult Invalid(string error) => new(false, false, error);
}
