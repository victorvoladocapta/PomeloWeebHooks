using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PomeloWeebHooks.Infrastructure.Pomelo;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloWebhookVerifierTests
{
    [Fact]
    public void Accepts_valid_pomelo_signature()
    {
        const string secret = "secret";
        const string timestamp = "1710000000";
        const string endpoint = "/api/webhooks/pomelo/cards/v1/cards/events";
        const string body = "{\"event\":\"ACTIVATION\"}";
        var signature = Hmac(secret, timestamp + endpoint + body);
        var verifier = CreateVerifier("api-key", secret);

        Assert.True(verifier.IsValid("api-key", signature, timestamp, endpoint, endpoint, body));
    }

    [Fact]
    public void Rejects_wrong_api_key_or_endpoint()
    {
        var verifier = CreateVerifier("api-key", "secret");

        Assert.False(verifier.IsValid("wrong", "00", "1", "/signed", "/actual", "{}"));
        Assert.False(verifier.IsValid("api-key", "00", "1", "/signed", "/actual", "{}"));
    }

    private static PomeloWebhookVerifier CreateVerifier(string apiKey, string secret) =>
        new(Options.Create(new PomeloWebhookOptions { ApiKey = apiKey, ApiSecret = secret }), new FakeEnvironment());

    private static string Hmac(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    private sealed class FakeEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
