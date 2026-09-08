using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PomeloWeebHooks.Application.Security;
using PomeloWeebHooks.Application.TransactionNotifications;

namespace PomeloWeebHooks.API.Controllers;

[ApiController, AllowAnonymous, Route("api/webhooks/pomelo/transactions/v1/notifications")]
public sealed class PomeloTransactionNotificationsWebhookController(
    IPomeloWebhookVerifier verifier,
    PomeloTransactionNotificationService service) : ControllerBase
{
    [HttpPost, Consumes("application/json")]
    public async Task<IActionResult> Receive(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? endpoint,
        CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);
        if (!verifier.IsValid(apiKey, signature, timestamp, endpoint, Request.Path + Request.QueryString, body))
            return Unauthorized();
        var result = await service.ProcessAsync(body, "network", ct);
        return result.IsValid ? Ok() : BadRequest(new { message = result.Error });
    }
}

[ApiController, AllowAnonymous, Route("api/webhooks/pomelo/transactions/on-us/notifications")]
public sealed class PomeloOnUsTransactionNotificationsWebhookController(
    IPomeloWebhookVerifier verifier,
    PomeloTransactionNotificationService service) : ControllerBase
{
    [HttpPost, Consumes("application/json")]
    public async Task<IActionResult> Receive(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? endpoint,
        CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);
        if (!verifier.IsValid(apiKey, signature, timestamp, endpoint, Request.Path + Request.QueryString, body))
            return Unauthorized();
        var result = await service.ProcessAsync(body, "on_us", ct);
        return result.IsValid ? Ok() : BadRequest(new { message = result.Error });
    }
}
