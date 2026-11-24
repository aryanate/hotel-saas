using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.Retention;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/retention")]
[Route("api/retention")]
[Authorize(Policy = "HotelAdmin")]
public class RetentionController : ControllerBase
{
    private readonly IMediator _mediator;

    public RetentionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("policies")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RetentionPolicyDto>>>> GetRetentionPolicies(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRetentionPoliciesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RetentionPolicyDto>>.Ok(result));
    }

    [HttpPut("policies")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RetentionPolicyDto>>>> UpdateRetentionPolicies(
        [FromBody] JsonElement body,
        CancellationToken cancellationToken)
    {
        List<UpdateRetentionPolicyRequest>? policiesList = null;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (body.ValueKind == JsonValueKind.Array)
        {
            policiesList = JsonSerializer.Deserialize<List<UpdateRetentionPolicyRequest>>(body.GetRawText(), options);
        }
        else if (body.TryGetProperty("policies", out var prop))
        {
            policiesList = JsonSerializer.Deserialize<List<UpdateRetentionPolicyRequest>>(prop.GetRawText(), options);
        }

        var result = await _mediator.Send(new UpdateRetentionPoliciesCommand(policiesList ?? new()), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RetentionPolicyDto>>.Ok(result, "Retention policies updated successfully."));
    }

    [HttpGet("preview")]
    [HttpGet("deletion-preview")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeletionPreviewDto>>>> GetDeletionPreview(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeletionPreviewQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DeletionPreviewDto>>.Ok(result));
    }
}
