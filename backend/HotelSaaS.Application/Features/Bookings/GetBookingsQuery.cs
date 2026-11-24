using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Application.Common.Models;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Bookings;

public record GetBookingsQuery : IRequest<PagedResult<BookingDto>>
{
    public BookingStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Search { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, PagedResult<BookingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBookingsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var query = _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .Where(b => b.HotelId == hotelId.Value && !b.IsDeleted);

        if (request.Status.HasValue)
        {
            query = query.Where(b => b.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(b => b.CheckInDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(b => b.CheckOutDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(b => b.BookingReference.Contains(request.Search) ||
                                     (b.Guest != null && (b.Guest.FullName.Contains(request.Search) || b.Guest.Mobile.Contains(request.Search))));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = bookings.Select(b => new BookingDto
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

        return new PagedResult<BookingDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
