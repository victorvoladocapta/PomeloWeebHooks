using System.Text.Json;
using System.Text.Json.Serialization;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Operations;

public sealed record RevertedOperationRequest(
    [property: JsonPropertyName("event_id")] string? EventId,
    [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey,
    [property: JsonPropertyName("data")] RevertedOperationData? Data);

public sealed record RevertedOperationData(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("credit_line_id")] string? CreditLineId,
    [property: JsonPropertyName("card_id")] string? CardId,
    [property: JsonPropertyName("card_last_four")] string? CardLastFour,
    [property: JsonPropertyName("user_id")] string? UserId,
    [property: JsonPropertyName("merchant_id")] string? MerchantId,
    [property: JsonPropertyName("merchant_name")] string? MerchantName,
    [property: JsonPropertyName("installments_quantity")] string? InstallmentsQuantity,
    [property: JsonPropertyName("reverted_date_time")] string? RevertedDateTime,
    [property: JsonPropertyName("local_amount")] RevertedAmount? LocalAmount);

public sealed record RevertedAmount(
    [property: JsonPropertyName("total")] string? Total,
    [property: JsonPropertyName("currency")] string? Currency);

public interface IPomeloRevertedOperationStore
{
    Task<bool> TryAddAsync(PomeloRevertedOperationEvent operationEvent, CancellationToken cancellationToken);
}

public sealed class PomeloRevertedOperationService(IPomeloRevertedOperationStore store)
{
    public async Task<RevertedOperationResult> ProcessAsync(string rawJson, CancellationToken ct)
    {
        RevertedOperationRequest? request;
        try { request = JsonSerializer.Deserialize<RevertedOperationRequest>(rawJson); }
        catch (JsonException) { return RevertedOperationResult.Invalid("El cuerpo JSON no es válido."); }
        var error = Validate(request);
        if (error is not null) return RevertedOperationResult.Invalid(error);
        var d = request!.Data!;
        var now = DateTimeOffset.UtcNow;
        var entity = new PomeloRevertedOperationEvent
        {
            Id = Guid.NewGuid(), EventId = request.EventId!.Trim(), IdempotencyKey = request.IdempotencyKey!.Trim(),
            OperationId = d.Id!.Trim(), Status = d.Status!.Trim().ToUpperInvariant(), CreditLineId = d.CreditLineId!.Trim(),
            CardId = d.CardId!.Trim(), CardLastFour = Clean(d.CardLastFour), UserId = d.UserId!.Trim(),
            MerchantId = Clean(d.MerchantId), MerchantName = Clean(d.MerchantName), InstallmentsQuantity = Clean(d.InstallmentsQuantity),
            RevertedDateTime = d.RevertedDateTime!.Trim(), LocalAmountTotal = d.LocalAmount!.Total!.Trim(),
            LocalAmountCurrency = d.LocalAmount.Currency!.Trim().ToUpperInvariant(), PayloadJson = rawJson,
            ReceivedAt = now, ProcessedAt = now,
        };
        return RevertedOperationResult.Accepted(!await store.TryAddAsync(entity, ct));
    }

    private static string? Validate(RevertedOperationRequest? r)
    {
        if (r is null) return "El cuerpo es requerido.";
        if (!string.Equals(r.EventId, "operation_reverted", StringComparison.OrdinalIgnoreCase)) return "event_id no es soportado.";
        if (string.IsNullOrWhiteSpace(r.IdempotencyKey)) return "idempotency_key es requerido.";
        if (r.Data is null) return "data es requerido.";
        var d = r.Data;
        if (string.IsNullOrWhiteSpace(d.Id)) return "data.id es requerido.";
        if (string.IsNullOrWhiteSpace(d.Status)) return "data.status es requerido.";
        if (string.IsNullOrWhiteSpace(d.CreditLineId)) return "data.credit_line_id es requerido.";
        if (string.IsNullOrWhiteSpace(d.CardId)) return "data.card_id es requerido.";
        if (string.IsNullOrWhiteSpace(d.UserId)) return "data.user_id es requerido.";
        if (string.IsNullOrWhiteSpace(d.RevertedDateTime)) return "data.reverted_date_time es requerido.";
        if (d.LocalAmount is null || string.IsNullOrWhiteSpace(d.LocalAmount.Total) || string.IsNullOrWhiteSpace(d.LocalAmount.Currency)) return "data.local_amount es requerido.";
        return null;
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record RevertedOperationResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static RevertedOperationResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static RevertedOperationResult Invalid(string error) => new(false, false, error);
}
