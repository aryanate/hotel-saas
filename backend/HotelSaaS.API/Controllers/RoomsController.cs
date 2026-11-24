using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.Rooms;
using HotelSaaS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/rooms")]
[Route("api/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomDto>>>> GetRooms(
        [FromQuery] RoomStatus? status,
        [FromQuery] Guid? roomTypeId,
        [FromQuery] int? floor,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = new GetRoomsQuery
        {
            Status = status,
            RoomTypeId = roomTypeId,
            Floor = floor,
            Search = search
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoomDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "HotelAdmin")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> CreateRoom(
        [FromBody] CreateRoomCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(result, "Room created successfully."));
    }

    [HttpGet("availability")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomDto>>>> GetRoomAvailability(
        [FromQuery] DateTime? checkInDate,
        [FromQuery] DateTime? checkOutDate,
        [FromQuery] DateTime? checkIn,
        [FromQuery] DateTime? checkOut,
        [FromQuery] Guid? roomTypeId,
        CancellationToken cancellationToken)
    {
        var start = checkInDate ?? checkIn ?? DateTime.UtcNow;
        var end = checkOutDate ?? checkOut ?? DateTime.UtcNow.AddDays(1);
        var query = new GetRoomAvailabilityQuery
        {
            CheckInDate = start,
            CheckOutDate = end,
            RoomTypeId = roomTypeId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoomDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> GetRoomById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRoomByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "HotelAdmin")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> UpdateRoom(
        [FromRoute] Guid id,
        [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoomCommand
        {
            Id = id,
            RoomNumber = request.RoomNumber,
            RoomTypeId = request.RoomTypeId,
            Floor = request.Floor,
            Capacity = request.Capacity,
            PricePerNight = request.PricePerNight,
            Description = request.Description,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(result, "Room updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "HotelAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRoom(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteRoomCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Room deleted successfully."));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> UpdateRoomStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateRoomStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoomStatusCommand
        {
            Id = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(result, "Room status updated successfully."));
    }

    [HttpGet("types")]
    [HttpGet("~/api/v1/room-types")]
    [HttpGet("~/api/room-types")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomTypeDto>>>> GetRoomTypes(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRoomTypesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoomTypeDto>>.Ok(result));
    }

    [HttpPost("types")]
    [HttpPost("~/api/v1/room-types")]
    [HttpPost("~/api/room-types")]
    [Authorize(Policy = "HotelAdmin")]
    public async Task<ActionResult<ApiResponse<RoomTypeDto>>> CreateRoomType(
        [FromBody] CreateRoomTypeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<RoomTypeDto>.Ok(result, "Room type created successfully."));
    }
}

public class UpdateRoomRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public Guid RoomTypeId { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string Description { get; set; } = string.Empty;
    public RoomStatus Status { get; set; }
}

public class UpdateRoomStatusRequest
{
    public RoomStatus Status { get; set; }
}
