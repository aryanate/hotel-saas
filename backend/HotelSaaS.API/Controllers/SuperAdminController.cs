using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.SuperAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Route("api/admin")]
[Authorize(Policy = "SuperAdmin")]
public class SuperAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuperAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("hotels")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminHotelDto>>>> GetAllHotels(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllHotelsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminHotelDto>>.Ok(result));
    }

    [HttpGet("hotels/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminHotelDto>>> GetHotelById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetHotelByIdAdminQuery(id), cancellationToken);
        return Ok(ApiResponse<AdminHotelDto>.Ok(result));
    }

    [HttpPut("hotels/{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateHotelStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateHotelStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (request.IsActive)
        {
            var res = await _mediator.Send(new ActivateHotelCommand(id), cancellationToken);
            return Ok(ApiResponse<bool>.Ok(res, "Hotel activated successfully."));
        }
        else
        {
            var res = await _mediator.Send(new SuspendHotelCommand(id), cancellationToken);
            return Ok(ApiResponse<bool>.Ok(res, "Hotel suspended successfully."));
        }
    }

    [HttpPut("hotels/{id:guid}/suspend")]
    public async Task<ActionResult<ApiResponse<bool>>> SuspendHotel(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SuspendHotelCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Hotel suspended successfully."));
    }

    [HttpPut("hotels/{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateHotel(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivateHotelCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Hotel activated successfully."));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<PlatformStatsDto>>> GetPlatformStats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlatformStatsQuery(), cancellationToken);
        return Ok(ApiResponse<PlatformStatsDto>.Ok(result));
    }

    [HttpGet("subscription-plans")]
    [HttpGet("plans")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubscriptionPlanDto>>>> GetSubscriptionPlans(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubscriptionPlansQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SubscriptionPlanDto>>.Ok(result));
    }

    [HttpPost("subscription-plans")]
    [HttpPost("plans")]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> CreateSubscriptionPlan(
        [FromBody] CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<SubscriptionPlanDto>.Ok(result, "Subscription plan created successfully."));
    }

    [HttpPut("subscription-plans/{id:guid}")]
    [HttpPut("plans/{id:guid}")]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> UpdateSubscriptionPlan(
        [FromRoute] Guid id,
        [FromBody] UpdateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSubscriptionPlanCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            BillingCycle = request.BillingCycle,
            MaxBookingsPerMonth = request.MaxBookingsPerMonth,
            FeaturesJson = request.FeaturesJson,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<SubscriptionPlanDto>.Ok(result, "Subscription plan updated successfully."));
    }
}

public class UpdateHotelStatusRequest
{
    public bool IsActive { get; set; }
}

public class UpdateSubscriptionPlanRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
    public int MaxBookingsPerMonth { get; set; }
    public string FeaturesJson { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
}
