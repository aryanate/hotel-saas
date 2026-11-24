using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record GetRoomAvailabilityQuery : IRequest<IReadOnlyList<RoomDto>>
{
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public Guid? RoomTypeId { get; init; }
}

public class GetRoomAvailabilityQueryHandler : IRequestHandler<GetRoomAvailabilityQuery, IReadOnlyList<RoomDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetRoomAvailabilityQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RoomDto>> Handle(GetRoomAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var bookedRoomIds = await _context.BookingRooms
            .Where(br => br.HotelId == hotelId.Value &&
                         br.Booking!.Status != BookingStatus.Cancelled &&
                         br.Booking!.Status != BookingStatus.CheckedOut &&
                         br.CheckInDate < request.CheckOutDate &&
                         br.CheckOutDate > request.CheckInDate)
            .Select(br => br.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = _context.Rooms
            .Include(r => r.RoomType)
            .Where(r => r.HotelId == hotelId.Value &&
                        !r.IsDeleted &&
                        r.Status == RoomStatus.Available &&
                        !bookedRoomIds.Contains(r.Id));

        if (request.RoomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == request.RoomTypeId.Value);
        }

        var availableRooms = await query
            .OrderBy(r => r.RoomNumber)
            .ToListAsync(cancellationToken);

        return availableRooms.Select(r => new RoomDto
        {
            Id = r.Id,
            HotelId = r.HotelId,
            RoomNumber = r.RoomNumber,
            RoomTypeId = r.RoomTypeId,
            RoomTypeName = r.RoomType?.Name,
            Floor = r.Floor,
            Capacity = r.Capacity,
            PricePerNight = r.PricePerNight,
            Description = r.Description,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();
    }
}
