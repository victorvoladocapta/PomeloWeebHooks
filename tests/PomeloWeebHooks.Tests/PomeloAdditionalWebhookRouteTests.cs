using Microsoft.AspNetCore.Mvc;
using PomeloWeebHooks.API.Controllers;

namespace PomeloWeebHooks.Tests;

public sealed class PomeloAdditionalWebhookRouteTests
{
    [Theory]
    [InlineData(nameof(PomeloAdditionalWebhooksController.TransactionNotifications), "transactions/v1/notifications")]
    [InlineData(nameof(PomeloAdditionalWebhooksController.PresentmentNotifications), "presentments/v1/notifications")]
    [InlineData(nameof(PomeloAdditionalWebhooksController.StatementOpened), "statements-opened")]
    [InlineData(nameof(PomeloAdditionalWebhooksController.InterestAndCharges), "interest")]
    [InlineData(nameof(PomeloAdditionalWebhooksController.UserStatusChanged), "identity/v1/users/status-changed")]
    public void Exposes_expected_public_route(string actionName, string expected)
    {
        var method = typeof(PomeloAdditionalWebhooksController).GetMethod(actionName)!;
        var attribute = method.GetCustomAttributes(typeof(HttpPostAttribute), true)
            .Cast<HttpPostAttribute>()
            .Single();
        Assert.Equal(expected, attribute.Template);
    }
}
