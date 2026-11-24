using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.CheckIn;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/checkin")]
[Route("api/checkin")]
[Authorize]
public class CheckInController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckInController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CheckInSessionDto>>>> GetPendingCheckIns(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingCheckInsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CheckInSessionDto>>.Ok(result));
    }

    [HttpPost("{sessionId:guid}/approve")]
    [HttpPost("sessions/{sessionId:guid}/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveCheckIn(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApproveCheckInCommand(sessionId), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Check-in approved successfully."));
    }

    [HttpPost("{sessionId:guid}/reject")]
    [HttpPost("sessions/{sessionId:guid}/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectCheckIn(
        [FromRoute] Guid sessionId,
        [FromBody] RejectCheckInRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new RejectCheckInCommand(sessionId, request?.Reason);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Check-in rejected."));
    }
}

public class RejectCheckInRequest
{
    public string? Reason { get; set; }
}
