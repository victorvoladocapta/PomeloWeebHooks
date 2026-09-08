using PomeloWeebHooks.Application.InboundEvents;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloInboundEventServiceTests
{
    public static TheoryData<PomeloInboundEventKind, string, string, string?> ValidPayloads => new()
    {
        {
            PomeloInboundEventKind.ProcessingTransaction,
            """{"event_id":"authorization-advice","idempotency_key":"ctx-key","event_detail":{"transaction":{"id":"ctx-1"},"user":{"id":"usr-1"},"status":"APPROVED"}}""",
            "ctx-1", "usr-1"
        },
        {
            PomeloInboundEventKind.Presentment,
            """{"event_id":"presentment-notification","idempotency_key":"cpr-key","event_detail":{"type":"PRESENTMENT","public_id":"cpr-1","user_id":"usr-1","status":"IN_REVIEW"}}""",
            "cpr-1", "usr-1"
        },
        {
            PomeloInboundEventKind.StatementOpened,
            """{"event_id":"statement_opened","idempotency_key":"lst-key","data":{"id":"lst-1","credit_line_id":"lcr-1","start_date":"2024-02-01","closing_date":"2024-02-29"}}""",
            "lst-1", null
        },
        {
            PomeloInboundEventKind.InterestOrCharge,
            """{"event_id":"interest_created","idempotency_key":"interest-key","data":{"id":"revolving:1","user_id":"usr-1","debt_id":"dbt-1","origin":"IMMEDIATE_CHARGE","type":"PROJECTION","credit_line_id":"lcr-1","effective_at":"2022-12-15T13:55:00","amount":{"total":"75.82","currency":"BRL"}}}""",
            "revolving:1", "usr-1"
        },
        {
            PomeloInboundEventKind.UserStatusChanged,
            """{"event_id":"user-status-changed","idempotency_key":"usr-key","data":{"user_id":"usr-1","status":"BLOCKED"}}""",
            "usr-1", "usr-1"
        },
    };

    [Theory]
    [MemberData(nameof(ValidPayloads))]
    public async Task Accepts_and_extracts_each_contract(
        PomeloInboundEventKind kind,
        string json,
        string resourceId,
        string? userId)
    {
        var store = new RecordingStore();
        var result = await new PomeloInboundEventService(store).ProcessAsync(json, kind, default);

        Assert.True(result.IsValid);
        Assert.Equal(resourceId, store.LastEvent!.ResourceId);
        Assert.Equal(userId, store.LastEvent.PomeloUserId);
        Assert.Equal(json, store.LastEvent.PayloadJson);
    }

    [Theory]
    [InlineData("{}", "event_id es requerido.")]
    [InlineData("{\"event_id\":\"x\"}", "idempotency_key es requerido.")]
    [InlineData("not-json", "El cuerpo JSON no es válido.")]
    public async Task Rejects_invalid_envelopes(string json, string expectedError)
    {
        var result = await new PomeloInboundEventService(new RecordingStore())
            .ProcessAsync(json, PomeloInboundEventKind.StatementOpened, default);
        Assert.False(result.IsValid);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task Reports_idempotent_replay_as_duplicate()
    {
        var store = new RecordingStore(StoreInboundEventOutcome.Duplicate);
        var result = await new PomeloInboundEventService(store).ProcessAsync(
            """{"event_id":"statement_opened","idempotency_key":"same","data":{"id":"lst-1"}}""",
            PomeloInboundEventKind.StatementOpened,
            default);
        Assert.True(result.IsValid);
        Assert.True(result.IsDuplicate);
    }

    private sealed class RecordingStore(StoreInboundEventOutcome outcome = StoreInboundEventOutcome.Created)
        : IPomeloInboundEventStore
    {
        public PomeloInboundEvent? LastEvent { get; private set; }

        public Task<StoreInboundEventOutcome> TryAddAsync(
            PomeloInboundEvent inboundEvent,
            CancellationToken cancellationToken)
        {
            LastEvent = inboundEvent;
            return Task.FromResult(outcome);
        }
    }
}
