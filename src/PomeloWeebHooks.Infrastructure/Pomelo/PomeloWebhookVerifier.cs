using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PomeloWeebHooks.Application.Security;

namespace PomeloWeebHooks.Infrastructure.Pomelo;

public sealed class PomeloWebhookVerifier(
    IOptions<PomeloWebhookOptions> options,
    IHostEnvironment environment) : IPomeloWebhookVerifier
{
    public bool IsValid(
        string? apiKey,
        string? signature,
        string? timestamp,
        string? signedEndpoint,
        string requestEndpoint,
        string rawBody)
    {
        var configuration = options.Value;
        if (environment.IsDevelopment()
            && configuration.AllowUnsignedInDevelopment
            && string.IsNullOrWhiteSpace(configuration.ApiSecret))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(configuration.ApiKey)
            || string.IsNullOrWhiteSpace(configuration.ApiSecret)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(signature)
            || string.IsNullOrWhiteSpace(timestamp)
            || string.IsNullOrWhiteSpace(signedEndpoint))
        {
            return false;
        }

        if (!FixedTimeEquals(configuration.ApiKey, apiKey.Trim()))
            return false;

        if (!EndpointsEqual(signedEndpoint, requestEndpoint))
            return false;

        var provided = NormalizeSignature(signature);
        var signedPayload = timestamp.Trim() + signedEndpoint.Trim() + rawBody;
        foreach (var key in SecretKeys(configuration.ApiSecret))
        {
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            if (SignatureEquals(hash, provided))
                return true;
        }

        return false;
    }

    private static bool EndpointsEqual(string signedEndpoint, string requestEndpoint)
    {
        static string Normalize(string value)
        {
            var trimmed = value.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
                trimmed = uri.PathAndQuery;
            return "/" + trimmed.Trim('/');
        }

        return string.Equals(Normalize(signedEndpoint), Normalize(requestEndpoint), StringComparison.Ordinal);
    }

    private static string NormalizeSignature(string signature)
    {
        var normalized = signature.Trim();
        if (normalized.StartsWith("hmac-sha256 ", StringComparison.OrdinalIgnoreCase))
            normalized = normalized["hmac-sha256 ".Length..].Trim();
        if (normalized.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            normalized = normalized["sha256=".Length..].Trim();
        return normalized;
    }

    private static IEnumerable<byte[]> SecretKeys(string secret)
    {
        yield return Encoding.UTF8.GetBytes(secret);
        byte[]? decoded = null;
        try { decoded = Convert.FromBase64String(secret); }
        catch (FormatException) { }
        if (decoded is { Length: > 0 }) yield return decoded;
    }

    private static bool SignatureEquals(byte[] expected, string provided)
    {
        try
        {
            var bytes = Convert.FromHexString(provided);
            if (bytes.Length == expected.Length)
                return CryptographicOperations.FixedTimeEquals(expected, bytes);
        }
        catch (FormatException) { }

        return FixedTimeEquals(Convert.ToBase64String(expected), provided);
    }

    private static bool FixedTimeEquals(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        return expectedBytes.Length == providedBytes.Length
               && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
