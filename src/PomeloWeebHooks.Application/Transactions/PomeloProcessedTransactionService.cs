using System.Text.Json;
using System.Text.Json.Serialization;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Transactions;

public sealed record ProcessedTransactionRequest(
    [property: JsonPropertyName("event_id")] string? EventId,
    [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey,
    [property: JsonPropertyName("data")] ProcessedTransactionData? Data);

public sealed record ProcessedTransactionData(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("status_detail")] string? StatusDetail,
    [property: JsonPropertyName("credit_line_id")] string? CreditLineId,
    [property: JsonPropertyName("card_id")] string? CardId,
    [property: JsonPropertyName("card_last_four")] string? CardLastFour,
    [property: JsonPropertyName("user_id")] string? UserId,
    [property: JsonPropertyName("merchant_id")] string? MerchantId,
    [property: JsonPropertyName("merchant_name")] string? MerchantName,
    [property: JsonPropertyName("installments_quantity")] string? InstallmentsQuantity,
    [property: JsonPropertyName("transaction_date_time")] string? TransactionDateTime,
    [property: JsonPropertyName("local_amount")] ProcessedTransactionAmount? LocalAmount);

public sealed record ProcessedTransactionAmount(
    [property: JsonPropertyName("total")] string? Total,
    [property: JsonPropertyName("currency")] string? Currency);

public interface IPomeloProcessedTransactionStore
{
    Task<bool> TryAddAsync(PomeloProcessedTransactionEvent transactionEvent, CancellationToken cancellationToken);
}

public sealed class PomeloProcessedTransactionService(IPomeloProcessedTransactionStore store)
{
    public async Task<ProcessedTransactionResult> ProcessAsync(string rawJson, CancellationToken cancellationToken)
    {
        ProcessedTransactionRequest? request;
        try { request = JsonSerializer.Deserialize<ProcessedTransactionRequest>(rawJson); }
        catch (JsonException) { return ProcessedTransactionResult.Invalid("El cuerpo JSON no es válido."); }

        var error = Validate(request);
        if (error is not null) return ProcessedTransactionResult.Invalid(error);
        var data = request!.Data!;
        var now = DateTimeOffset.UtcNow;
        var entity = new PomeloProcessedTransactionEvent
        {
            Id = Guid.NewGuid(), IdempotencyKey = request.IdempotencyKey!.Trim(), EventId = request.EventId!.Trim(),
            TransactionId = data.Id!.Trim(), Status = data.Status!.Trim().ToUpperInvariant(),
            StatusDetail = data.StatusDetail!.Trim().ToUpperInvariant(), CreditLineId = data.CreditLineId!.Trim(),
            CardId = data.CardId!.Trim(), CardLastFour = Clean(data.CardLastFour), UserId = data.UserId!.Trim(),
            MerchantId = Clean(data.MerchantId), MerchantName = Clean(data.MerchantName),
            InstallmentsQuantity = Clean(data.InstallmentsQuantity), TransactionDateTime = data.TransactionDateTime!.Trim(),
            LocalAmountTotal = data.LocalAmount!.Total!.Trim(), LocalAmountCurrency = data.LocalAmount.Currency!.Trim().ToUpperInvariant(),
            PayloadJson = rawJson, ReceivedAt = now, ProcessedAt = now,
        };
        var created = await store.TryAddAsync(entity, cancellationToken);
        return ProcessedTransactionResult.Accepted(!created);
    }

    private static string? Validate(ProcessedTransactionRequest? r)
    {
        if (r is null) return "El cuerpo es requerido.";
        if (!string.Equals(r.EventId, "transaction_processed", StringComparison.OrdinalIgnoreCase)) return "event_id no es soportado.";
        if (string.IsNullOrWhiteSpace(r.IdempotencyKey)) return "idempotency_key es requerido.";
        if (r.Data is null) return "data es requerido.";
        var d = r.Data;
        if (string.IsNullOrWhiteSpace(d.Id)) return "data.id es requerido.";
        if (string.IsNullOrWhiteSpace(d.Status)) return "data.status es requerido.";
        if (string.IsNullOrWhiteSpace(d.StatusDetail)) return "data.status_detail es requerido.";
        if (string.IsNullOrWhiteSpace(d.CreditLineId)) return "data.credit_line_id es requerido.";
        if (string.IsNullOrWhiteSpace(d.CardId)) return "data.card_id es requerido.";
        if (string.IsNullOrWhiteSpace(d.UserId)) return "data.user_id es requerido.";
        if (string.IsNullOrWhiteSpace(d.TransactionDateTime)) return "data.transaction_date_time es requerido.";
        if (d.LocalAmount is null || string.IsNullOrWhiteSpace(d.LocalAmount.Total) || string.IsNullOrWhiteSpace(d.LocalAmount.Currency)) return "data.local_amount es requerido.";
        return null;
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record ProcessedTransactionResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static ProcessedTransactionResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static ProcessedTransactionResult Invalid(string error) => new(false, false, error);
}
