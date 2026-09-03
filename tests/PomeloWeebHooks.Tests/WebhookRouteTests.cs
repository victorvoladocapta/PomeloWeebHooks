using Microsoft.AspNetCore.Mvc;
using PomeloWeebHooks.API.Controllers;

namespace PomeloWeebHooks.Tests;

public sealed class WebhookRouteTests
{
    [Theory]
    [InlineData(typeof(PomeloCreditLineStatusWebhookController), "api/webhooks/pomelo/credit-lines/status-changed")]
    [InlineData(typeof(PomeloStatementCreatedWebhookController), "statements")]
    [InlineData(typeof(PomeloProcessedTransactionsWebhookController), "api/webhooks/pomelo/lending/transactions")]
    [InlineData(typeof(PomeloRevertedOperationsWebhookController), "api/webhooks/pomelo/reverted-operations")]
    [InlineData(typeof(PomeloDelinquencyWebhookController), "lending/v1/debt")]
    public void Lending_webhook_exposes_expected_route(Type controllerType, string expected)
    {
        var route = controllerType.GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .Cast<RouteAttribute>()
            .Single();

        Assert.Equal(expected, route.Template);
    }
}
