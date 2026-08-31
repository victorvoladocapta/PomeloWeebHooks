using Microsoft.EntityFrameworkCore;
using Npgsql;
using PomeloWeebHooks.Application.CreditLineStatus;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class PomeloCreditLineStatusStore(WebhookDbContext dbContext) : IPomeloCreditLineStatusStore
{
    public async Task<StoreCreditLineStatusOutcome> TryAddAsync(
        PomeloCreditLineStatusEvent statusEvent,
        CancellationToken cancellationToken)
    {
        if (await dbContext.PomeloCreditLineStatusEvents.AsNoTracking()
                .AnyAsync(x => x.IdempotencyKey == statusEvent.IdempotencyKey, cancellationToken))
        {
            return StoreCreditLineStatusOutcome.Duplicate;
        }

        dbContext.PomeloCreditLineStatusEvents.Add(statusEvent);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return StoreCreditLineStatusOutcome.Created;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(statusEvent).State = EntityState.Detached;
            return StoreCreditLineStatusOutcome.Duplicate;
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
