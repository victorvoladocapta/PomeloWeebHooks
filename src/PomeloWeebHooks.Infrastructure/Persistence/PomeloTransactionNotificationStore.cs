using Microsoft.EntityFrameworkCore;
using Npgsql;
using PomeloWeebHooks.Application.TransactionNotifications;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class PomeloTransactionNotificationStore(WebhookDbContext db) : IPomeloTransactionNotificationStore
{
    public async Task<bool> TryAddAsync(PomeloTransactionNotificationEvent entity, CancellationToken ct)
    {
        if (await db.PomeloTransactionNotificationEvents.AsNoTracking().AnyAsync(x => x.IdempotencyKey == entity.IdempotencyKey, ct))
            return false;
        db.PomeloTransactionNotificationEvents.Add(entity);
        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateException ex) when (Unique(ex)) { db.Entry(entity).State = EntityState.Detached; return false; }
    }

    private static bool Unique(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
            if (current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) return true;
        return false;
    }
}
