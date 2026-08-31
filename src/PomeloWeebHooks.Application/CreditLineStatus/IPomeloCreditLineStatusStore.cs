using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.CreditLineStatus;

public interface IPomeloCreditLineStatusStore
{
    Task<StoreCreditLineStatusOutcome> TryAddAsync(
        PomeloCreditLineStatusEvent statusEvent,
        CancellationToken cancellationToken);
}

public enum StoreCreditLineStatusOutcome
{
    Created,
    Duplicate,
}
