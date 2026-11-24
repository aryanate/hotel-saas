using HotelSaaS.API.Common;
using HotelSaaS.Application.Common.Models;
using HotelSaaS.Application.Features.Bookings;
using HotelSaaS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetBookings(
        [FromQuery] BookingStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBookingsQuery
        {
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<BookingDto>>.Ok(result));
    }

    [HttpGet("today")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BookingDto>>>> GetTodayBookings(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTodayBookingsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BookingDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> GetBookingById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingDto>>> CreateBooking(
        [FromBody] CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(result, "Booking created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> UpdateBooking(
        [FromRoute] Guid id,
        [FromBody] UpdateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBookingCommand
        {
            Id = id,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            NumberOfAdults = request.NumberOfAdults,
            NumberOfChildren = request.NumberOfChildren,
            SpecialRequests = request.SpecialRequests,
            RoomIds = request.RoomIds
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(result, "Booking updated successfully."));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> ConfirmBooking(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmBookingCommand(id), cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(result, "Booking confirmed successfully."));
    }

    [HttpPost("{id:guid}/checkin")]
    [HttpPost("{id:guid}/check-in")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> CheckIn(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckInCommand(id), cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(result, "Guest checked in successfully."));
    }

    [HttpPost("{id:guid}/checkout")]
    [HttpPost("{id:guid}/check-out")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> CheckOut(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckOutCommand(id), cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(result, "Guest checked out successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> CancelBooking(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelBookingCommand(id), cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(result, "Booking cancelled successfully."));
    }

    [HttpGet("{id:guid}/checkin-link")]
    public async Task<ActionResult<ApiResponse<string>>> GetCheckInLink(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCheckInLinkQuery(id), cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    [HttpPost("{id:guid}/checkin-link")]
    [HttpPost("{id:guid}/send-checkin-link")]
    public async Task<ActionResult<ApiResponse<object>>> SendCheckInLink(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendCheckInLinkCommand(id), cancellationToken);
        var link = await _mediator.Send(new GetCheckInLinkQuery(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { success = result, checkInUrl = link }, "Check-in link sent successfully."));
    }
}

public class UpdateBookingRequest
{
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfAdults { get; set; } = 1;
    public int NumberOfChildren { get; set; }
    public string? SpecialRequests { get; set; }
    public List<Guid>? RoomIds { get; set; }
}
