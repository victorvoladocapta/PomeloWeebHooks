using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PomeloWeebHooks.Application.Security;
using PomeloWeebHooks.Application.Statements;

namespace PomeloWeebHooks.API.Controllers;

[ApiController, AllowAnonymous, Route("api/webhooks/pomelo/statements/opened")]
public sealed class PomeloStatementOpenedWebhookController(
    IPomeloWebhookVerifier verifier,
    PomeloStatementOpenedService service) : ControllerBase
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
        var result = await service.ProcessAsync(body, ct);
        return result.IsValid ? Ok() : BadRequest(new { message = result.Error });
    }
}
