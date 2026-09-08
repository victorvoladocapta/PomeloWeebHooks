using Microsoft.EntityFrameworkCore;
using Npgsql;
using PomeloWeebHooks.Application.CardEvents;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class PomeloCardEventStore(WebhookDbContext dbContext) : IPomeloCardEventStore
{
    public async Task<StoreCardEventOutcome> TryAddAsync(
        PomeloCardEvent cardEvent,
        CancellationToken cancellationToken)
    {
        if (await dbContext.PomeloCardEvents.AsNoTracking()
                .AnyAsync(x => x.IdempotencyKey == cardEvent.IdempotencyKey, cancellationToken))
        {
            return StoreCardEventOutcome.Duplicate;
        }

        dbContext.PomeloCardEvents.Add(cardEvent);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return StoreCardEventOutcome.Created;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(cardEvent).State = EntityState.Detached;
            return StoreCardEventOutcome.Duplicate;
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
