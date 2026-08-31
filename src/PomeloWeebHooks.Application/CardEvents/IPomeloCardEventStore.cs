using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.CardEvents;

public enum StoreCardEventOutcome
{
    Created,
    Duplicate,
}

public interface IPomeloCardEventStore
{
    Task<StoreCardEventOutcome> TryAddAsync(PomeloCardEvent cardEvent, CancellationToken cancellationToken);
}
