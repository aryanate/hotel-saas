using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.Webhooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("whatsapp")]
    public async Task<IActionResult> VerifyWhatsApp(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VerifyWhatsAppWebhookQuery(mode ?? string.Empty, verifyToken ?? string.Empty, challenge ?? string.Empty), cancellationToken);
        if (result != null)
        {
            return Content(result, "text/plain");
        }

        return Forbid();
    }

    [HttpPost("whatsapp")]
    public async Task<ActionResult<ApiResponse<bool>>> HandleWhatsApp(
        [FromBody] object payload,
        CancellationToken cancellationToken)
    {
        var rawJson = payload?.ToString() ?? string.Empty;
        var result = await _mediator.Send(new HandleWhatsAppWebhookCommand(rawJson), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Webhook processed."));
    }
}
