using Microsoft.EntityFrameworkCore;
using Npgsql;
using PomeloWeebHooks.Application.InboundEvents;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class PomeloInboundEventStore(WebhookDbContext dbContext) : IPomeloInboundEventStore
{
    public async Task<StoreInboundEventOutcome> TryAddAsync(
        PomeloInboundEvent inboundEvent,
        CancellationToken cancellationToken)
    {
        if (await dbContext.PomeloInboundEvents.AsNoTracking().AnyAsync(
                x => x.Kind == inboundEvent.Kind && x.IdempotencyKey == inboundEvent.IdempotencyKey,
                cancellationToken))
            return StoreInboundEventOutcome.Duplicate;

        dbContext.PomeloInboundEvents.Add(inboundEvent);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return StoreInboundEventOutcome.Created;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(inboundEvent).State = EntityState.Detached;
            return StoreInboundEventOutcome.Duplicate;
        }
    }

    private static bool IsUniqueViolation(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                return true;
        }
        return false;
    }
}
