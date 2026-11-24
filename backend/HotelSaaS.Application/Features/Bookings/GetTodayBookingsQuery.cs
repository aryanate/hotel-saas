using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Bookings;

public record GetTodayBookingsQuery : IRequest<IReadOnlyList<BookingDto>>;

public class GetTodayBookingsQueryHandler : IRequestHandler<GetTodayBookingsQuery, IReadOnlyList<BookingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTodayBookingsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BookingDto>> Handle(GetTodayBookingsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var bookings = await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .Where(b => b.HotelId == hotelId.Value && !b.IsDeleted &&
                        ((b.CheckInDate >= today && b.CheckInDate < tomorrow) ||
                         (b.CheckOutDate >= today && b.CheckOutDate < tomorrow)))
            .OrderBy(b => b.CheckInDate)
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
