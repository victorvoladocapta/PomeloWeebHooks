using PomeloWeebHooks.Application.Delinquency;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloDelinquencyServiceTests
{
    [Fact]
    public async Task Accepts_user_in_arrears()
    {
        var store = new FakeStore();
        const string payload = """{"event_id":"user_in_arrears","idempotency_key":"key-1","data":{"user_id":"usr-1","credit_line_id":"lcr-1","effective_at":"2022-12-15T13:55:00"}}""";
        var result = await new PomeloDelinquencyService(store).ProcessAsync(payload, default);
        Assert.True(result.IsValid);
        Assert.Equal("user_in_arrears", store.Event!.EventId);
        Assert.Equal("lcr-1", store.Event.CreditLineId);
    }

    [Fact]
    public async Task Rejects_missing_effective_at()
    {
        const string payload = """{"event_id":"user_in_arrears","idempotency_key":"key","data":{"user_id":"usr","credit_line_id":"lcr"}}""";
        var result = await new PomeloDelinquencyService(new FakeStore()).ProcessAsync(payload, default);
        Assert.False(result.IsValid);
        Assert.Contains("effective_at", result.Error);
    }

    private sealed class FakeStore : IPomeloDelinquencyStore
    {
        public PomeloDelinquencyEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloDelinquencyEvent delinquencyEvent, CancellationToken cancellationToken)
        { Event = delinquencyEvent; return Task.FromResult(true); }
    }
}
