using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PomeloWeebHooks.Application.InboundEvents;
using PomeloWeebHooks.Application.Security;

namespace PomeloWeebHooks.API.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class PomeloAdditionalWebhooksController(
    IPomeloWebhookVerifier verifier,
    PomeloInboundEventService service,
    ILogger<PomeloAdditionalWebhooksController> logger) : ControllerBase
{
    [HttpPost("transactions/v1/notifications")]
    public Task<IActionResult> TransactionNotifications(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? endpoint,
        CancellationToken cancellationToken) =>
        Receive(PomeloInboundEventKind.ProcessingTransaction, apiKey, signature, timestamp, endpoint, cancellationToken);

    [HttpPost("presentments/v1/notifications")]
    public Task<IActionResult> PresentmentNotifications(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? endpoint,
        CancellationToken cancellationToken) =>
        Receive(PomeloInboundEventKind.Presentment, apiKey, signature, timestamp, endpoint, cancellationToken);

    [HttpPost("statements-opened")]
    public Task<IActionResult> StatementOpened(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? endpoint,
        CancellationToken cancellationToken) =>
        Receive(PomeloInboundEventKind.StatementOpened, apiKey, signature, timestamp, endpoint, cancellationToken);

    [HttpPost("interest")]
    public Task<IActionResult> InterestAndCharges(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? endpoint,
        CancellationToken cancellationToken) =>
        Receive(PomeloInboundEventKind.InterestOrCharge, apiKey, signature, timestamp, endpoint, cancellationToken);

    [HttpPost("identity/v1/users/status-changed")]
    public Task<IActionResult> UserStatusChanged(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? endpoint,
        CancellationToken cancellationToken) =>
        Receive(PomeloInboundEventKind.UserStatusChanged, apiKey, signature, timestamp, endpoint, cancellationToken);

    private async Task<IActionResult> Receive(
        PomeloInboundEventKind kind,
        string? apiKey,
        string? signature,
        string? timestamp,
        string? signedEndpoint,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);
        var requestEndpoint = Request.Path + Request.QueryString;
        if (!verifier.IsValid(apiKey, signature, timestamp, signedEndpoint, requestEndpoint, body))
        {
            logger.LogWarning("Webhook Pomelo {WebhookKind} rechazado por firma o headers inválidos", kind);
            return Unauthorized();
        }

        var result = await service.ProcessAsync(body, kind, cancellationToken);
        return result.IsValid ? Ok() : BadRequest(new { message = result.Error });
    }
}
