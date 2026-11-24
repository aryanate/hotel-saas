using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Bookings;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto>;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBookingByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var b = await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.HotelId == hotelId.Value && !b.IsDeleted, cancellationToken);

        if (b == null)
        {
            throw new EntityNotFoundException("Booking", request.Id);
        }

        return new BookingDto
        {
            Id = b.Id,
            HotelId = b.HotelId,
            GuestId = b.GuestId,
            GuestName = b.Guest?.FullName ?? "",
            GuestMobile = b.Guest?.Mobile ?? "",
            GuestEmail = b.Guest?.Email,
            BookingReference = b.BookingReference,
            Source = b.Source,
            Status = b.Status,
            NumberOfAdults = b.NumberOfAdults,
            NumberOfChildren = b.NumberOfChildren,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            TotalNights = b.TotalNights,
            SpecialRequests = b.SpecialRequests,
            TotalAmount = b.BookingRooms.Sum(br => br.PricePerNight * Math.Max(1, (br.CheckOutDate - br.CheckInDate).Days)),
            Rooms = b.BookingRooms.Select(br => new BookingRoomDto
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room?.RoomNumber ?? "",
                RoomTypeName = br.Room?.RoomType?.Name,
                PricePerNight = br.PricePerNight,
                CheckInDate = br.CheckInDate,
                CheckOutDate = br.CheckOutDate
            }).ToList(),
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };
    }
}
