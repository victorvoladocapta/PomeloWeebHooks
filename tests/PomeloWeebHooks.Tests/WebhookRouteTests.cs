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
    [InlineData(typeof(PomeloTransactionNotificationsWebhookController), "api/webhooks/pomelo/transactions/v1/notifications")]
    [InlineData(typeof(PomeloOnUsTransactionNotificationsWebhookController), "api/webhooks/pomelo/transactions/on-us/notifications")]
    [InlineData(typeof(PomeloPresentmentsWebhookController), "api/webhooks/pomelo/presentments/v1/notifications")]
    [InlineData(typeof(PomeloStatementOpenedWebhookController), "api/webhooks/pomelo/statements/opened")]
    [InlineData(typeof(PomeloInterestChargesWebhookController), "api/webhooks/pomelo/credit-lines/interest-charges")]
    [InlineData(typeof(PomeloUserStatusWebhookController), "api/webhooks/pomelo/users/v1/status-changed")]
    [InlineData(typeof(PomeloShippingWebhookController), "api/webhooks/pomelo/shipping/updates")]
    [InlineData(typeof(PomeloChargebackWebhookController), "api/webhooks/pomelo/chargebacks")]
    public void Lending_webhook_exposes_expected_route(Type controllerType, string expected)
    {
        var route = controllerType.GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .Cast<RouteAttribute>()
            .Single();

        Assert.Equal(expected, route.Template);
    }
}
