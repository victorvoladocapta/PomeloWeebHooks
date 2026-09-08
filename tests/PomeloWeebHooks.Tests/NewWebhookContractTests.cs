using PomeloWeebHooks.Application.Chargebacks;
using PomeloWeebHooks.Application.InterestCharges;
using PomeloWeebHooks.Application.Presentments;
using PomeloWeebHooks.Application.Shipping;
using PomeloWeebHooks.Application.Statements;
using PomeloWeebHooks.Application.TransactionNotifications;
using PomeloWeebHooks.Application.UserStatus;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class NewWebhookContractTests
{
    [Fact]
    public async Task Transaction_notification_accepts_authorization_advice()
    {
        var store = new FakeTxStore();
        var svc = new PomeloTransactionNotificationService(store);
        const string json = """
            {"event_id":"authorization-advice","idempotency_key":"k1","event_detail":{"status":"REJECTED","user":{"id":"usr-1"},"card":{"id":"crd-1"},"merchant":{"name":"OXXO"},"amount":{"local":{"total":"10.00","currency":"MXN"}},"transaction":{"id":"ctx-1"}}}
            """;
        var result = await svc.ProcessAsync(json, "network", default);
        Assert.True(result.IsValid);
        Assert.False(result.IsDuplicate);
        Assert.NotNull(store.Event);
        Assert.Equal("network", store.Event!.Variant);
    }

    [Fact]
    public async Task Presentment_accepts_notification()
    {
        var store = new FakePresentmentStore();
        var svc = new PomeloPresentmentService(store);
        const string json = """
            {"event_id":"presentment-notification","event_detail":{"id":"cpr-1","user_id":"usr-1","card_id":"crd-1","status":"APPROVED","amounts":{"transaction_amount":{"amount":"10","currency":"MXN"}}}}
            """;
        var result = await svc.ProcessAsync(json, default);
        Assert.True(result.IsValid);
        Assert.Equal("cpr-1", store.Event!.IdempotencyKey);
    }

    [Fact]
    public async Task Statement_opened_accepts_contract()
    {
        var store = new FakeOpenedStore();
        var svc = new PomeloStatementOpenedService(store);
        const string json = """
            {"event_id":"statement_opened","idempotency_key":"lst-1-opened","data":{"id":"lst-1","credit_line_id":"lcr-1","start_date":"2024-02-01","closing_date":"2024-02-29"}}
            """;
        var result = await svc.ProcessAsync(json, default);
        Assert.True(result.IsValid);
        Assert.Equal("lst-1", store.Event!.StatementId);
    }

    [Fact]
    public async Task Interest_created_accepts_contract()
    {
        var store = new FakeInterestStore();
        var svc = new PomeloInterestChargeService(store);
        const string json = """
            {"event_id":"interest_created","idempotency_key":"i1","data":{"id":"revolving:1","user_id":"usr-1","origin":"IMMEDIATE_CHARGE","amount":{"total":"1.00","currency":"MXN"}}}
            """;
        var result = await svc.ProcessAsync(json, default);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task User_status_accepts_contract()
    {
        var store = new FakeUserStore();
        var svc = new PomeloUserStatusService(store);
        const string json = """
            {"event_id":"users_status_changed","idempotency_key":"u1","user_id":"usr-1","user_status":"BLOCKED","user_status_reason":"CLIENT_INTERNAL_REASON"}
            """;
        var result = await svc.ProcessAsync(json, default);
        Assert.True(result.IsValid);
        Assert.Equal("BLOCKED", store.Event!.UserStatus);
    }

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

    private sealed class FakeTxStore : IPomeloTransactionNotificationStore
    {
        public PomeloTransactionNotificationEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloTransactionNotificationEvent entity, CancellationToken cancellationToken)
        {
            Event = entity;
            return Task.FromResult(true);
        }
    }

    private sealed class FakePresentmentStore : IPomeloPresentmentStore
    {
        public PomeloPresentmentEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloPresentmentEvent entity, CancellationToken cancellationToken)
        {
            Event = entity;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeOpenedStore : IPomeloStatementOpenedStore
    {
        public PomeloStatementOpenedEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloStatementOpenedEvent entity, CancellationToken cancellationToken)
        {
            Event = entity;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeInterestStore : IPomeloInterestChargeStore
    {
        public Task<bool> TryAddAsync(PomeloInterestChargeEvent entity, CancellationToken cancellationToken)
            => Task.FromResult(true);
    }

    private sealed class FakeUserStore : IPomeloUserStatusStore
    {
        public PomeloUserStatusEvent? Event { get; private set; }
        public Task<bool> TryAddAsync(PomeloUserStatusEvent entity, CancellationToken cancellationToken)
        {
            Event = entity;
            return Task.FromResult(true);
        }
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
