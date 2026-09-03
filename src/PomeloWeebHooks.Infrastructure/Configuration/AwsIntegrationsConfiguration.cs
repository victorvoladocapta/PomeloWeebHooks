using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace PomeloWeebHooks.Infrastructure.Configuration;

/// <summary>
/// Loads the Pomelo webhook credentials from the Secrets Manager entry the AWS
/// stack points the container at, and hands them to the service as ordinary
/// configuration.
///
/// Keys in the secret are configuration paths with a double underscore for the
/// separator -- <c>Pomelo__ApiKey</c>, <c>Pomelo__ApiSecret</c> -- which is the
/// same convention environment variables use. That keeps this class from having
/// to know which keys exist: adding one is a change to the secret, not to this
/// code.
/// </summary>
public static class AwsIntegrationsConfiguration
{
    private const string SecretArnVariable = "INTEGRATIONS_SECRET_ARN";

    /// <summary>
    /// The settings the secret carries, or null when there is nothing to add:
    /// off AWS, or on AWS before anyone has written a value.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, string?>?> BuildAsync(
        CancellationToken ct = default)
    {
        var secretArn = Environment.GetEnvironmentVariable(SecretArnVariable);
        if (string.IsNullOrWhiteSpace(secretArn))
            return null;

        string? payload;
        try
        {
            using var client = new AmazonSecretsManagerClient();
            var response = await client.GetSecretValueAsync(
                new GetSecretValueRequest { SecretId = secretArn },
                ct);
            payload = response.SecretString;
        }
        catch (ResourceNotFoundException)
        {
            // Terraform creates the secret empty and someone fills it later, so
            // an entry with no version yet is an ordinary state and not a fault.
            // The service still refuses to start without a signing secret, which
            // is the check that actually matters and lives in Program.cs.
            return null;
        }

        if (string.IsNullOrWhiteSpace(payload))
            return null;

        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                $"The secret behind {SecretArnVariable} must be a JSON object of " +
                "configuration keys, for example {\"Pomelo__ApiKey\": \"...\"}.");
        }

        var settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in document.RootElement.EnumerateObject())
        {
            var value = entry.Value.ValueKind switch
            {
                JsonValueKind.String => entry.Value.GetString(),
                JsonValueKind.Null => null,
                _ => entry.Value.GetRawText(),
            };

            // A blank entry means the credential has not been supplied, not that
            // it should be blanked out. Skipping it leaves whatever appsettings
            // declares, so a half-filled secret cannot erase a working value.
            if (string.IsNullOrWhiteSpace(value))
                continue;

            settings[entry.Name.Replace("__", ":")] = value;
        }

        return settings.Count == 0 ? null : settings;
    }
}
