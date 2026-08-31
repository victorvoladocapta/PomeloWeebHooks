using System.Text.Json;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.CreditLineStatus;

public sealed class PomeloCreditLineStatusService(IPomeloCreditLineStatusStore store)
{
    public async Task<CreditLineStatusResult> ProcessAsync(
        string rawJson,
        CancellationToken cancellationToken)
    {
        PomeloCreditLineStatusRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<PomeloCreditLineStatusRequest>(rawJson);
        }
        catch (JsonException)
        {
            return CreditLineStatusResult.Invalid("El cuerpo JSON no es válido.");
        }

        var validationError = Validate(request);
        if (validationError is not null)
            return CreditLineStatusResult.Invalid(validationError);

        var now = DateTimeOffset.UtcNow;
        var statusEvent = new PomeloCreditLineStatusEvent
        {
            Id = Guid.NewGuid(),
            EventId = request!.EventId!.Trim(),
            IdempotencyKey = request.IdempotencyKey!.Trim(),
            CreditLineId = request.Data!.CreditLineId!.Trim(),
            Status = request.Data.Status!.Trim().ToUpperInvariant(),
            Reason = NullIfWhiteSpace(request.Data.Reason)?.ToUpperInvariant(),
            PayloadJson = rawJson,
            ReceivedAt = now,
            ProcessedAt = now,
        };

        var outcome = await store.TryAddAsync(statusEvent, cancellationToken);
        return CreditLineStatusResult.Accepted(outcome == StoreCreditLineStatusOutcome.Duplicate);
    }

    private static string? Validate(PomeloCreditLineStatusRequest? request)
    {
        if (request is null) return "El cuerpo es requerido.";
        if (string.IsNullOrWhiteSpace(request.EventId)) return "event_id es requerido.";
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) return "idempotency_key es requerido.";
        if (request.Data is null) return "data es requerido.";
        if (string.IsNullOrWhiteSpace(request.Data.CreditLineId)) return "data.credit_line_id es requerido.";
        if (string.IsNullOrWhiteSpace(request.Data.Status)) return "data.status es requerido.";
        return null;
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record CreditLineStatusResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static CreditLineStatusResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static CreditLineStatusResult Invalid(string error) => new(false, false, error);
}
