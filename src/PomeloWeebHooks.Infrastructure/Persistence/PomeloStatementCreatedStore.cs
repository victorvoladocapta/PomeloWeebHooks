using Microsoft.EntityFrameworkCore;
using Npgsql;
using PomeloWeebHooks.Application.Statements;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class PomeloStatementCreatedStore(WebhookDbContext dbContext) : IPomeloStatementCreatedStore
{
    public async Task<StoreStatementOutcome> TryAddAsync(
        PomeloStatementCreatedEvent statementEvent,
        CancellationToken cancellationToken)
    {
        if (await dbContext.PomeloStatementCreatedEvents.AsNoTracking()
                .AnyAsync(x => x.IdempotencyKey == statementEvent.IdempotencyKey, cancellationToken))
            return StoreStatementOutcome.Duplicate;

        dbContext.PomeloStatementCreatedEvents.Add(statementEvent);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return StoreStatementOutcome.Created;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(statementEvent).State = EntityState.Detached;
            return StoreStatementOutcome.Duplicate;
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
