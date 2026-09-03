using System.Reflection;
using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Npgsql;

namespace PomeloWeebHooks.Infrastructure.Configuration;

/// <summary>
/// Translates the environment the AWS stack hands the container into the
/// configuration keys <see cref="DependencyInjection.AddInfrastructure"/>
/// already reads.
///
/// The stack never injects the database password. It passes the ARN of the
/// Secrets Manager entry and grants the instance role permission to read it,
/// so the credential exists only inside the process that needs it: not in the
/// launch template, the instance metadata, the image, or Terraform state, and
/// it can be rotated without rebuilding any of them.
///
/// Outside AWS the DB_SECRET_ARN variable is absent and this does nothing, so
/// local development keeps working from ConnectionStrings__PostgreSql exactly
/// as the README describes.
/// </summary>
public static class AwsDatabaseConfiguration
{
    private const string SecretArnVariable = "DB_SECRET_ARN";

    private const string CaBundleResource =
        "PomeloWeebHooks.Infrastructure.Certificates.Amazon_RDS_Global_Root_CA_bundle.pem";

    /// <summary>
    /// The settings the AWS environment implies, or null when not running
    /// against it.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, string?>?> BuildAsync(
        CancellationToken ct = default)
    {
        var secretArn = Environment.GetEnvironmentVariable(SecretArnVariable);
        if (string.IsNullOrWhiteSpace(secretArn))
            return null;

        var (username, password) = await ReadCredentialAsync(secretArn, ct);

        var connection = new NpgsqlConnectionStringBuilder
        {
            Host = Require("DB_HOST"),
            Port = ReadPort(),
            Database = Require("DB_NAME"),
            Username = username,
            Password = password,
            SslMode = ReadSslMode(),
            Timeout = 60,
            CommandTimeout = 60,
            KeepAlive = 60,
            Pooling = true,
        };

        // The two AWS endpoints this service can be pointed at are signed by
        // different authorities, and neither anchor alone covers both. A direct
        // RDS instance presents a per-region Amazon RDS root that is absent from
        // the operating system trust store; RDS Proxy presents an Amazon Trust
        // Services certificate that is in that store but absent from the RDS
        // bundle. Npgsql's root certificate setting replaces the system store
        // rather than adding to it, so naming only one of them breaks the other
        // -- and it breaks it as a connection error that reads like a firewall
        // problem rather than a trust problem. This cost a full afternoon of
        // chasing security groups on the sibling service; it is not theoretical.
        if (connection.SslMode is SslMode.VerifyCA or SslMode.VerifyFull)
            connection.RootCertificate = BuildTrustStore();

        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgreSql"] = connection.ConnectionString,
        };
    }

    private static async Task<(string Username, string Password)> ReadCredentialAsync(
        string secretArn,
        CancellationToken ct)
    {
        using var client = new AmazonSecretsManagerClient();

        var response = await client.GetSecretValueAsync(
            new GetSecretValueRequest { SecretId = secretArn },
            ct);

        if (string.IsNullOrWhiteSpace(response.SecretString))
            throw new InvalidOperationException(
                $"{SecretArnVariable} points at a secret with no string value. " +
                "Terraform creates the container; the credential has to be written into it.");

        using var document = JsonDocument.Parse(response.SecretString);
        var root = document.RootElement;

        if (!root.TryGetProperty("username", out var username) ||
            !root.TryGetProperty("password", out var password))
        {
            throw new InvalidOperationException(
                $"The secret behind {SecretArnVariable} must be a JSON object with " +
                "\"username\" and \"password\".");
        }

        return (username.GetString() ?? string.Empty, password.GetString() ?? string.Empty);
    }

    /// <summary>
    /// Writes the bundled Amazon RDS roots, followed by whatever the operating
    /// system already trusts, to a single file Npgsql can open.
    /// </summary>
    private static string BuildTrustStore()
    {
        var path = Path.Combine(Path.GetTempPath(), "postgres-trust-store.pem");

        using var file = File.Create(path);

        using (var bundled = Assembly.GetExecutingAssembly()
                                 .GetManifestResourceStream(CaBundleResource)
                             ?? throw new InvalidOperationException(
                                 $"Embedded resource {CaBundleResource} is missing from the build."))
        {
            bundled.CopyTo(file);
        }

        // Absent on distributions that lay the trust store out differently. That
        // is survivable: the bundled roots still cover a direct RDS connection,
        // and the failure it would cause is a clear certificate error rather
        // than a silently weakened one.
        foreach (var systemStore in SystemTrustStores)
        {
            if (!File.Exists(systemStore))
                continue;

            file.WriteByte((byte)'\n');
            using var roots = File.OpenRead(systemStore);
            roots.CopyTo(file);
            break;
        }

        return path;
    }

    private static readonly string[] SystemTrustStores =
    [
        "/etc/ssl/certs/ca-certificates.crt",
        "/etc/pki/tls/certs/ca-bundle.crt",
    ];

    private static SslMode ReadSslMode()
    {
        var configured = Environment.GetEnvironmentVariable("DB_SSL_MODE");

        // Verifying the certificate and the hostname is the default on purpose:
        // an unset variable must not silently weaken the connection.
        if (string.IsNullOrWhiteSpace(configured))
            return SslMode.VerifyFull;

        return Enum.TryParse<SslMode>(configured, ignoreCase: true, out var parsed)
            ? parsed
            : throw new InvalidOperationException(
                $"DB_SSL_MODE has the unrecognised value '{configured}'. " +
                $"Expected one of: {string.Join(", ", Enum.GetNames<SslMode>())}.");
    }

    private static int ReadPort()
    {
        var configured = Environment.GetEnvironmentVariable("DB_PORT");
        if (string.IsNullOrWhiteSpace(configured))
            return 5432;

        return int.TryParse(configured, out var port)
            ? port
            : throw new InvalidOperationException(
                $"DB_PORT has the non-numeric value '{configured}'.");
    }

    private static string Require(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"{variable} is not set. It is required whenever {SecretArnVariable} is, " +
                "because together they describe where the database is.");

        return value;
    }
}
