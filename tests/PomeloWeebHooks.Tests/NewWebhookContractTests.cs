using PomeloWeebHooks.Application.Chargebacks;
using PomeloWeebHooks.Application.Shipping;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class NewWebhookContractTests
{
    [Fact]
    public async Task Shipping_update_accepts_contract()
    {
        var store = new FakeShippingStore();
        var svc = new PomeloShippingService(store);
        const string json = """
            {"event_id":"shipment-status-changed","idempotency_key":"s1","shipment_id":"shi-1","status":"IN_TRANSIT","status_detail":"RECEIVED_BY_CARRIER"}
            """;
        var result = await svc.ProcessAsync(json, default);
        Assert.True(result.IsValid);
        Assert.Equal("shi-1", store.Event!.ShipmentId);
        Assert.Equal("IN_TRANSIT", store.Event.Status);
    }

    [Fact]
    public async Task Chargeback_notification_accepts_contract()
    {
        var store = new FakeChargebackStore();
        var svc = new PomeloChargebackService(store);
        const string json = """
            {"event_id":"chargeback_notification","idempotency_key":"cb1","id":"cbk-1","transaction_id":"ctx-1","status":"DISPUTE_WON","status_ticket":"DONE","amount":10,"currency":"MXN"}
            """;
        var result = await svc.ProcessAsync(json, default);
        Assert.True(result.IsValid);
        Assert.Equal("cbk-1", store.Event!.ChargebackId);
        Assert.Equal("DISPUTE_WON", store.Event.Status);
    }

    private sealed class FakeShippingStore : IPomeloShippingStore
    {
        public PomeloShippingEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloShippingEvent entity, CancellationToken cancellationToken)
        {
            Event = entity;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeChargebackStore : IPomeloChargebackStore
    {
        public PomeloChargebackEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloChargebackEvent entity, CancellationToken cancellationToken)
        {
            Event = entity;
            return Task.FromResult(true);
        }
    }
}
