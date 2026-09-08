using Microsoft.EntityFrameworkCore;
using Npgsql;
using PomeloWeebHooks.Application.UserStatus;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class PomeloUserStatusStore(WebhookDbContext db) : IPomeloUserStatusStore
{
    public async Task<bool> TryAddAsync(PomeloUserStatusEvent entity, CancellationToken ct)
    {
        if (await db.PomeloUserStatusEvents.AsNoTracking().AnyAsync(x => x.IdempotencyKey == entity.IdempotencyKey, ct))
            return false;
        db.PomeloUserStatusEvents.Add(entity);
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
