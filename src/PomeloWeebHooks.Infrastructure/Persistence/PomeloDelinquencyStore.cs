using Microsoft.EntityFrameworkCore;
using Npgsql;
using PomeloWeebHooks.Application.Delinquency;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class PomeloDelinquencyStore(WebhookDbContext db) : IPomeloDelinquencyStore
{
    public async Task<bool> TryAddAsync(PomeloDelinquencyEvent entity, CancellationToken ct)
    {
        if (await db.PomeloDelinquencyEvents.AsNoTracking().AnyAsync(x => x.IdempotencyKey == entity.IdempotencyKey, ct)) return false;
        db.PomeloDelinquencyEvents.Add(entity);
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
