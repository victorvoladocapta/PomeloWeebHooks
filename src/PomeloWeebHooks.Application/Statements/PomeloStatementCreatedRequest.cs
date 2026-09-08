using System.Text.Json.Serialization;

namespace PomeloWeebHooks.Application.Statements;

public sealed record PomeloStatementCreatedRequest(
    [property: JsonPropertyName("event_id")] string? EventId,
    [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey,
    [property: JsonPropertyName("data")] PomeloStatementCreatedData? Data);

public sealed record PomeloStatementCreatedData(
    [property: JsonPropertyName("id")] string? StatementId,
    [property: JsonPropertyName("credit_line_id")] string? CreditLineId);
