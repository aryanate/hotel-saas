using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Application.Features.Bookings;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record GetGuestHistoryQuery(Guid GuestId) : IRequest<IReadOnlyList<BookingDto>>;

public class GetGuestHistoryQueryHandler : IRequestHandler<GetGuestHistoryQuery, IReadOnlyList<BookingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGuestHistoryQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BookingDto>> Handle(GetGuestHistoryQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var guestExists = await _context.Guests
            .AnyAsync(g => g.Id == request.GuestId && g.HotelId == hotelId.Value && !g.IsDeleted, cancellationToken);

        if (!guestExists)
        {
            throw new EntityNotFoundException("Guest", request.GuestId);
        }

        var bookings = await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .Where(b => b.HotelId == hotelId.Value && b.GuestId == request.GuestId && !b.IsDeleted)
            .OrderByDescending(b => b.CheckInDate)
            .ToListAsync(cancellationToken);

        return bookings.Select(b => new BookingDto
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
        }).ToList();
    }
}
