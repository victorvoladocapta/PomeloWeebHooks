using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Application.Statements;

public interface IPomeloStatementCreatedStore
{
    Task<StoreStatementOutcome> TryAddAsync(
        PomeloStatementCreatedEvent statementEvent,
        CancellationToken cancellationToken);
}

public enum StoreStatementOutcome
{
    Created,
    Duplicate,
}
