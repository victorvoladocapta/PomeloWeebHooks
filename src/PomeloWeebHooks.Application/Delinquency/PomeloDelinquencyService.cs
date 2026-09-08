using System.Text.Json;
using System.Text.Json.Serialization;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Delinquency;

public sealed record DelinquencyRequest(
    [property: JsonPropertyName("event_id")] string? EventId,
    [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey,
    [property: JsonPropertyName("data")] DelinquencyData? Data);

public sealed record DelinquencyData(
    [property: JsonPropertyName("user_id")] string? UserId,
    [property: JsonPropertyName("credit_line_id")] string? CreditLineId,
    [property: JsonPropertyName("effective_at")] string? EffectiveAt);

public interface IPomeloDelinquencyStore
{
    Task<bool> TryAddAsync(PomeloDelinquencyEvent delinquencyEvent, CancellationToken cancellationToken);
}

public sealed class PomeloDelinquencyService(IPomeloDelinquencyStore store)
{
    public async Task<DelinquencyResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        DelinquencyRequest? request;
        try { request = JsonSerializer.Deserialize<DelinquencyRequest>(rawJson); }
        catch (JsonException) { return DelinquencyResult.Invalid("El cuerpo JSON no es válido."); }
        var error = Validate(request);
        if (error is not null) return DelinquencyResult.Invalid(error);
        var now = DateTimeOffset.UtcNow;
        var entity = new PomeloDelinquencyEvent
        {
            Id = Guid.NewGuid(), EventId = request!.EventId!.Trim().ToLowerInvariant(),
            IdempotencyKey = request.IdempotencyKey!.Trim(), UserId = request.Data!.UserId!.Trim(),
            CreditLineId = request.Data.CreditLineId!.Trim(), EffectiveAt = request.Data.EffectiveAt!.Trim(),
            PayloadJson = rawJson, ReceivedAt = now, ProcessedAt = now,
        };
        return DelinquencyResult.Accepted(!await store.TryAddAsync(entity, ct));
    }

    private static string? Validate(DelinquencyRequest? r)
    {
        if (r is null) return "El cuerpo es requerido.";
        if (string.IsNullOrWhiteSpace(r.EventId)) return "event_id es requerido.";
        if (string.IsNullOrWhiteSpace(r.IdempotencyKey)) return "idempotency_key es requerido.";
        if (r.Data is null) return "data es requerido.";
        if (string.IsNullOrWhiteSpace(r.Data.UserId)) return "data.user_id es requerido.";
        if (string.IsNullOrWhiteSpace(r.Data.CreditLineId)) return "data.credit_line_id es requerido.";
        if (string.IsNullOrWhiteSpace(r.Data.EffectiveAt)) return "data.effective_at es requerido.";
        return null;
    }
}

public sealed record DelinquencyResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static DelinquencyResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static DelinquencyResult Invalid(string error) => new(false, false, error);
}
