namespace PomeloWeebHooks.Application.Common;

public sealed record IngestResult(bool IsValid, bool IsDuplicate, string? Error)
{
    public static IngestResult Accepted(bool duplicate) => new(true, duplicate, null);
    public static IngestResult Invalid(string error) => new(false, false, error);
}
