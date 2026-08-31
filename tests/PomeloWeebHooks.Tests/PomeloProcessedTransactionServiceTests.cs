using PomeloWeebHooks.Application.Transactions;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloProcessedTransactionServiceTests
{
    [Fact]
    public async Task Accepts_documented_contract()
    {
        var store = new FakeStore();
        var result = await new PomeloProcessedTransactionService(store).ProcessAsync(Payload, default);
        Assert.True(result.IsValid);
        Assert.Equal("ctx-1", store.Event!.TransactionId);
        Assert.Equal("75.82", store.Event.LocalAmountTotal);
    }

    [Fact]
    public async Task Rejects_missing_amount()
    {
        const string invalid = """{"event_id":"transaction_processed","idempotency_key":"key","data":{"id":"ctx","status":"APPROVED","status_detail":"APPROVED","credit_line_id":"lcr","card_id":"crd","user_id":"usr","transaction_date_time":"2023-05-16"}}""";
        var result = await new PomeloProcessedTransactionService(new FakeStore()).ProcessAsync(invalid, default);
        Assert.False(result.IsValid);
        Assert.Contains("local_amount", result.Error);
    }

    private const string Payload = """
        {"event_id":"transaction_processed","idempotency_key":"ctx-1","data":{"id":"ctx-1","status":"APPROVED","status_detail":"APPROVED","credit_line_id":"lcr-1","card_id":"crd-1","card_last_four":"5439","user_id":"usr-1","merchant_id":"BGQ","merchant_name":"Lending Store","installments_quantity":"5","transaction_date_time":"2023-05-16T14:00:00","local_amount":{"total":"75.82","currency":"BRL"}}}
        """;

    private sealed class FakeStore : IPomeloProcessedTransactionStore
    {
        public PomeloProcessedTransactionEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloProcessedTransactionEvent transactionEvent, CancellationToken cancellationToken)
        { Event = transactionEvent; return Task.FromResult(true); }
    }
}
