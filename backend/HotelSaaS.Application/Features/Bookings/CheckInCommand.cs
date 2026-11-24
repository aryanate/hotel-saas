using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Bookings;

public record CheckInCommand(Guid Id) : IRequest<BookingDto>;

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, BookingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CheckInCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BookingDto> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var booking = await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.HotelId == hotelId.Value && !b.IsDeleted, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException("Booking", request.Id);
        }

        if (booking.Status == BookingStatus.CheckedIn)
        {
            throw new BusinessRuleViolationException("Booking is already checked in.", "ALREADY_CHECKED_IN");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new BusinessRuleViolationException("Cannot check in a cancelled booking.", "BOOKING_CANCELLED");
        }

        booking.Status = BookingStatus.CheckedIn;
        booking.UpdatedBy = _currentUserService.UserId;
        booking.UpdatedAt = DateTime.UtcNow;

        if (booking.Guest != null)
        {
            booking.Guest.TotalVisits++;
            booking.Guest.LastVisitAt = DateTime.UtcNow;
        }

        foreach (var br in booking.BookingRooms)
        {
            if (br.Room != null)
            {
                br.Room.Status = RoomStatus.Occupied;
                br.Room.UpdatedBy = _currentUserService.UserId;
                br.Room.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BookingDto
        {
            Id = booking.Id,
            HotelId = booking.HotelId,
            GuestId = booking.GuestId,
            GuestName = booking.Guest?.FullName ?? "",
            GuestMobile = booking.Guest?.Mobile ?? "",
            GuestEmail = booking.Guest?.Email,
            BookingReference = booking.BookingReference,
            Source = booking.Source,
            Status = booking.Status,
            NumberOfAdults = booking.NumberOfAdults,
            NumberOfChildren = booking.NumberOfChildren,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            TotalNights = booking.TotalNights,
            SpecialRequests = booking.SpecialRequests,
            TotalAmount = booking.BookingRooms.Sum(br => br.PricePerNight * Math.Max(1, (br.CheckOutDate - br.CheckInDate).Days)),
            Rooms = booking.BookingRooms.Select(br => new BookingRoomDto
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room?.RoomNumber ?? "",
                RoomTypeName = br.Room?.RoomType?.Name,
                PricePerNight = br.PricePerNight,
                CheckInDate = br.CheckInDate,
                CheckOutDate = br.CheckOutDate
            }).ToList(),
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }
}
