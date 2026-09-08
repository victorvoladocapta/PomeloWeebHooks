using PomeloWeebHooks.Application.Statements;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloStatementCreatedServiceTests
{
    [Fact]
    public async Task Accepts_statement_created_contract()
    {
        var store = new FakeStore();
        var service = new PomeloStatementCreatedService(store);

        var result = await service.ProcessAsync(ValidPayload, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.False(result.IsDuplicate);
        Assert.Equal("lst-1", store.LastEvent!.StatementId);
        Assert.Equal("lcr-1", store.LastEvent.CreditLineId);
    }

    [Fact]
    public async Task Duplicate_is_successful()
    {
        var service = new PomeloStatementCreatedService(
            new FakeStore { Outcome = StoreStatementOutcome.Duplicate });

        var result = await service.ProcessAsync(ValidPayload, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.True(result.IsDuplicate);
    }

    [Fact]
    public async Task Rejects_missing_statement_id()
    {
        var service = new PomeloStatementCreatedService(new FakeStore());
        const string payload = """
            {"event_id":"statement_created","idempotency_key":"key","data":{"credit_line_id":"lcr-1"}}
            """;

        var result = await service.ProcessAsync(payload, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains("data.id", result.Error);
    }

    private const string ValidPayload = """
        {"event_id":"statement_created","idempotency_key":"lst-1-generated","data":{"id":"lst-1","credit_line_id":"lcr-1"}}
        """;

    private sealed class FakeStore : IPomeloStatementCreatedStore
    {
        public StoreStatementOutcome Outcome { get; init; } = StoreStatementOutcome.Created;
        public PomeloStatementCreatedEvent? LastEvent { get; private set; }

        public Task<StoreStatementOutcome> TryAddAsync(
            PomeloStatementCreatedEvent statementEvent,
            CancellationToken cancellationToken)
        {
            LastEvent = statementEvent;
            return Task.FromResult(Outcome);
        }
    }
}
