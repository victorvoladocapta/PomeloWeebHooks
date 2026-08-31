using PomeloWeebHooks.Application.Operations;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloRevertedOperationServiceTests
{
    [Fact]
    public async Task Accepts_documented_contract()
    {
        var store = new FakeStore();
        var result = await new PomeloRevertedOperationService(store).ProcessAsync(Payload, default);
        Assert.True(result.IsValid);
        Assert.Equal("ctx-1", store.Event!.OperationId);
        Assert.Equal("REVERTED", store.Event.Status);
    }

    [Fact]
    public async Task Rejects_missing_reverted_date()
    {
        const string invalid = """{"event_id":"operation_reverted","idempotency_key":"key","data":{"id":"ctx","status":"REVERTED","credit_line_id":"lcr","card_id":"crd","user_id":"usr","local_amount":{"total":"1","currency":"MXN"}}}""";
        var result = await new PomeloRevertedOperationService(new FakeStore()).ProcessAsync(invalid, default);
        Assert.False(result.IsValid);
        Assert.Contains("reverted_date_time", result.Error);
    }

    private const string Payload = """
      {"event_id":"operation_reverted","idempotency_key":"ctx-1","data":{"id":"ctx-1","status":"REVERTED","credit_line_id":"lcr-1","card_id":"crd-1","card_last_four":"5439","user_id":"usr-1","merchant_id":"BGQ","merchant_name":"Lending Store","installments_quantity":"5","reverted_date_time":"2023-05-16T14:00:00","local_amount":{"total":"75.82","currency":"BRL"}}}
      """;

    private sealed class FakeStore : IPomeloRevertedOperationStore
    {
        public PomeloRevertedOperationEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloRevertedOperationEvent operationEvent, CancellationToken cancellationToken)
        { Event = operationEvent; return Task.FromResult(true); }
    }
}
