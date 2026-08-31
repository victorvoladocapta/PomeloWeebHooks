using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PomeloWeebHooks.Application.Security;
using PomeloWeebHooks.Application.Statements;

namespace PomeloWeebHooks.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/webhooks/pomelo/statements/created-summaries")]
public sealed class PomeloStatementCreatedWebhookController(
    IPomeloWebhookVerifier verifier,
    PomeloStatementCreatedService service,
    ILogger<PomeloStatementCreatedWebhookController> logger) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Receive(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Endpoint")] string? signedEndpoint,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        var requestEndpoint = Request.Path + Request.QueryString;

        if (!verifier.IsValid(apiKey, signature, timestamp, signedEndpoint, requestEndpoint, rawBody))
        {
            logger.LogWarning("Pomelo Statement Created rechazado por firma o headers inválidos");
            return Unauthorized();
        }

        var result = await service.ProcessAsync(rawBody, cancellationToken);
        if (!result.IsValid)
            return BadRequest(new { message = result.Error });

        return Ok();
    }
}
