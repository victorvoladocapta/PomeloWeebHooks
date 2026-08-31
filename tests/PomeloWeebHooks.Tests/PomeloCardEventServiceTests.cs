using PomeloWeebHooks.Application.CardEvents;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloCardEventServiceTests
{
    [Theory]
    [InlineData("CREATION")]
    [InlineData("ACTIVATION")]
    [InlineData("EMBOSSMENT")]
    [InlineData("BLOCK")]
    [InlineData("UNBLOCK")]
    [InlineData("DISABLEMENT")]
    public async Task Accepts_supported_card_events(string eventType)
    {
        var store = new FakeStore();
        var service = new PomeloCardEventService(store);
        var json = Payload(eventType, "key-1");

        var result = await service.ProcessAsync(json, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.False(result.IsDuplicate);
        Assert.Equal(eventType, store.LastEvent!.EventType);
        Assert.Equal("crd-1", store.LastEvent.PomeloCardId);
    }

    [Fact]
    public async Task Returns_success_for_duplicate()
    {
        var store = new FakeStore { Outcome = StoreCardEventOutcome.Duplicate };
        var service = new PomeloCardEventService(store);

        var result = await service.ProcessAsync(Payload("ACTIVATION", "same-key"), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.True(result.IsDuplicate);
    }

    [Fact]
    public async Task Rejects_missing_idempotency_key()
    {
        var service = new PomeloCardEventService(new FakeStore());
        const string json = """
            {"event_id":"card-notification","id":"crd-1","updated_at":"2026-08-30T12:00:00Z","user_id":"usr-1","event":"BLOCK","card_type":"VIRTUAL"}
            """;

        var result = await service.ProcessAsync(json, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains("idempotency_key", result.Error);
    }

    private static string Payload(string eventType, string key) => $$"""
        {"event_id":"card-notification","id":"crd-1","updated_at":"2026-08-30T12:00:00Z","user_id":"usr-1","event":"{{eventType}}","card_type":"VIRTUAL","related_card_id":"crd-1","idempotency_key":"{{key}}"}
        """;

    private sealed class FakeStore : IPomeloCardEventStore
    {
        public StoreCardEventOutcome Outcome { get; init; } = StoreCardEventOutcome.Created;
        public PomeloCardEvent? LastEvent { get; private set; }

        public Task<StoreCardEventOutcome> TryAddAsync(
            PomeloCardEvent cardEvent,
            CancellationToken cancellationToken)
        {
            LastEvent = cardEvent;
            return Task.FromResult(Outcome);
        }
    }
}
