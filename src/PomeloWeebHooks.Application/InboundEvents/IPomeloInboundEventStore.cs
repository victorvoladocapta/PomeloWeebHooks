using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.InboundEvents;

public interface IPomeloInboundEventStore
{
    Task<StoreInboundEventOutcome> TryAddAsync(PomeloInboundEvent inboundEvent, CancellationToken cancellationToken);
}

public enum StoreInboundEventOutcome
{
    Created,
    Duplicate,
}
