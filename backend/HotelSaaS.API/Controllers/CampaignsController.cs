using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.Campaigns;
using HotelSaaS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/campaigns")]
[Route("api/campaigns")]
[Authorize(Policy = "HotelAdmin")]
public class CampaignsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CampaignsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignDto>>>> GetCampaigns(
        [FromQuery] CampaignStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCampaignsQuery { Status = status }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CampaignDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> CreateCampaign(
        [FromBody] CreateCampaignCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<CampaignDto>.Ok(result, "Campaign created successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> GetCampaignById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCampaignByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<CampaignDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> UpdateCampaign(
        [FromRoute] Guid id,
        [FromBody] UpdateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCampaignCommand
        {
            Id = id,
            Name = request.Name,
            Occasion = request.Occasion,
            MessageTemplate = request.MessageTemplate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<CampaignDto>.Ok(result, "Campaign updated successfully."));
    }

    [HttpPost("{id:guid}/schedule")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> ScheduleCampaign(
        [FromRoute] Guid id,
        [FromBody] ScheduleCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleCampaignCommand(id, request.ScheduledAt);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<CampaignDto>.Ok(result, "Campaign scheduled successfully."));
    }

    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> SendCampaignNow(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new SendCampaignNowCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<CampaignDto>.Ok(result, "Campaign sending started."));
    }

    [HttpGet("{id:guid}/recipients")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignRecipientDto>>>> GetCampaignRecipients(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCampaignRecipientsQuery(id), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CampaignRecipientDto>>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCampaign(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCampaignCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Campaign deleted successfully."));
    }
}

public class UpdateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
}

public class ScheduleCampaignRequest
{
    public DateTime ScheduledAt { get; set; }
}
