using PomeloWeebHooks.Application.CreditLineStatus;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloCreditLineStatusServiceTests
{
    [Fact]
    public async Task Accepts_status_change_contract()
    {
        var store = new FakeStore();
        var service = new PomeloCreditLineStatusService(store);
        const string payload = """
            {
              "event_id": "credit_line_paused",
              "idempotency_key": "event-key-1",
              "data": {
                "credit_line_id": "lcr-1",
                "status": "PAUSED",
                "reason": "IN_ARREARS"
              }
            }
            """;

        var result = await service.ProcessAsync(payload, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.False(result.IsDuplicate);
        Assert.Equal("lcr-1", store.LastEvent!.CreditLineId);
        Assert.Equal("PAUSED", store.LastEvent.Status);
        Assert.Equal("IN_ARREARS", store.LastEvent.Reason);
    }

    [Fact]
    public async Task Duplicate_is_successful()
    {
        var service = new PomeloCreditLineStatusService(
            new FakeStore { Outcome = StoreCreditLineStatusOutcome.Duplicate });

        var result = await service.ProcessAsync(ValidPayload, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.True(result.IsDuplicate);
    }

    [Fact]
    public async Task Rejects_missing_credit_line_id()
    {
        var service = new PomeloCreditLineStatusService(new FakeStore());
        const string payload = """
            {"event_id":"credit_line_paused","idempotency_key":"key","data":{"status":"PAUSED"}}
            """;

        var result = await service.ProcessAsync(payload, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains("credit_line_id", result.Error);
    }

    private const string ValidPayload = """
        {"event_id":"credit_line_paused","idempotency_key":"key","data":{"credit_line_id":"lcr-1","status":"PAUSED"}}
        """;

    private sealed class FakeStore : IPomeloCreditLineStatusStore
    {
        public StoreCreditLineStatusOutcome Outcome { get; init; } = StoreCreditLineStatusOutcome.Created;
        public PomeloCreditLineStatusEvent? LastEvent { get; private set; }

        public Task<StoreCreditLineStatusOutcome> TryAddAsync(
            PomeloCreditLineStatusEvent statusEvent,
            CancellationToken cancellationToken)
        {
            LastEvent = statusEvent;
            return Task.FromResult(Outcome);
        }
    }
}
