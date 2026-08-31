namespace PomeloWeebHooks.Application.Security;

public interface IPomeloWebhookVerifier
{
    bool IsValid(
        string? apiKey,
        string? signature,
        string? timestamp,
        string? signedEndpoint,
        string requestEndpoint,
        string rawBody);
}
