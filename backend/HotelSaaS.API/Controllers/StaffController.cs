using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.Staff;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/staff")]
[Route("api/staff")]
[Authorize(Policy = "HotelAdmin")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StaffDto>>>> GetStaff(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStaffQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StaffDto>>.Ok(result));
    }

    [HttpPost]
    [HttpPost("invite")]
    public async Task<ActionResult<ApiResponse<StaffDto>>> InviteStaff(
        [FromBody] InviteStaffCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<StaffDto>.Ok(result, "Staff member invited successfully."));
    }

    [HttpGet("roles")]
    public ActionResult<ApiResponse<IReadOnlyList<RoleItemDto>>> GetAvailableRoles()
    {
        var roles = new List<RoleItemDto>
        {
            new RoleItemDto { Id = "HotelAdmin", Name = "Hotel Admin", Description = "Full hotel administrative access" },
            new RoleItemDto { Id = "Receptionist", Name = "Receptionist", Description = "Front desk, check-in, bookings" },
            new RoleItemDto { Id = "Housekeeping", Name = "Housekeeping", Description = "Room cleaning and status management" },
            new RoleItemDto { Id = "Accountant", Name = "Accountant", Description = "Invoicing, GST reports, payments" }
        };
        return Ok(ApiResponse<IReadOnlyList<RoleItemDto>>.Ok(roles));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StaffDto>>> UpdateStaff(
        [FromRoute] Guid id,
        [FromBody] UpdateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStaffCommand
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Mobile = request.Mobile,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<StaffDto>.Ok(result, "Staff member updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveStaff(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveStaffCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Staff member removed successfully."));
    }

    [HttpGet("{id:guid}/roles")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StaffRoleDto>>>> GetStaffRoles(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStaffRolesQuery(id), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StaffRoleDto>>.Ok(result));
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<ActionResult<ApiResponse<StaffDto>>> AssignRoles(
        [FromRoute] Guid id,
        [FromBody] AssignRolesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRolesCommand(id, request.Roles);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<StaffDto>.Ok(result, "Roles assigned successfully."));
    }
}

public class RoleItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateStaffRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AssignRolesRequest
{
    public List<string> Roles { get; set; } = new();
}
