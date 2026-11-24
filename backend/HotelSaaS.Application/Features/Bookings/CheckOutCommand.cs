using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Bookings;

public record CheckOutCommand(Guid Id) : IRequest<BookingDto>;

public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, BookingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CheckOutCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BookingDto> Handle(CheckOutCommand request, CancellationToken cancellationToken)
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

        if (booking.Status == BookingStatus.CheckedOut)
        {
            throw new BusinessRuleViolationException("Booking is already checked out.", "ALREADY_CHECKED_OUT");
        }

        booking.Status = BookingStatus.CheckedOut;
        booking.UpdatedBy = _currentUserService.UserId;
        booking.UpdatedAt = DateTime.UtcNow;

        foreach (var br in booking.BookingRooms)
        {
            if (br.Room != null)
            {
                br.Room.Status = RoomStatus.Cleaning;
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
