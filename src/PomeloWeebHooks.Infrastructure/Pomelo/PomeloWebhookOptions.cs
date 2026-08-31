namespace PomeloWeebHooks.Infrastructure.Pomelo;

public sealed class PomeloWebhookOptions
{
    public const string SectionName = "Pomelo";

    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool AllowUnsignedInDevelopment { get; set; }
}
