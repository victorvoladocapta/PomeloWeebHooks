using System.Text.Json;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Statements;

public sealed class PomeloStatementCreatedService(IPomeloStatementCreatedStore store)
{
    public async Task<StatementCreatedResult> ProcessAsync(
        string rawJson,
        CancellationToken cancellationToken)
    {
        PomeloStatementCreatedRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<PomeloStatementCreatedRequest>(rawJson);
        }
        catch (JsonException)
        {
            return StatementCreatedResult.Invalid("El cuerpo JSON no es válido.");
        }

        var validationError = Validate(request);
        if (validationError is not null)
            return StatementCreatedResult.Invalid(validationError);

        var now = DateTimeOffset.UtcNow;
        var entity = new PomeloStatementCreatedEvent
        {
            Id = Guid.NewGuid(),
            EventId = request!.EventId!.Trim(),
            IdempotencyKey = request.IdempotencyKey!.Trim(),
            StatementId = request.Data!.StatementId!.Trim(),
            CreditLineId = request.Data.CreditLineId!.Trim(),
            PayloadJson = rawJson,
            ReceivedAt = now,
            ProcessedAt = now,
        };

        var outcome = await store.TryAddAsync(entity, cancellationToken);
        return StatementCreatedResult.Accepted(outcome == StoreStatementOutcome.Duplicate);
    }

    private static string? Validate(PomeloStatementCreatedRequest? request)
    {
        if (request is null) return "El cuerpo es requerido.";
        if (string.IsNullOrWhiteSpace(request.EventId)) return "event_id es requerido.";
        if (!string.Equals(request.EventId, "statement_created", StringComparison.OrdinalIgnoreCase))
            return "event_id no es soportado.";
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) return "idempotency_key es requerido.";
        if (request.Data is null) return "data es requerido.";
        if (string.IsNullOrWhiteSpace(request.Data.StatementId)) return "data.id es requerido.";
        if (string.IsNullOrWhiteSpace(request.Data.CreditLineId)) return "data.credit_line_id es requerido.";
        return null;
    }
}

public sealed record StatementCreatedResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static StatementCreatedResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static StatementCreatedResult Invalid(string error) => new(false, false, error);
}
