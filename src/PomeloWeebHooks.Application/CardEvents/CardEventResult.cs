namespace PomeloWeebHooks.Application.CardEvents;

public sealed record CardEventResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static CardEventResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static CardEventResult Invalid(string error) => new(false, false, error);
}
