using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record GetRoomTypesQuery : IRequest<IReadOnlyList<RoomTypeDto>>;

public class GetRoomTypesQueryHandler : IRequestHandler<GetRoomTypesQuery, IReadOnlyList<RoomTypeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetRoomTypesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RoomTypeDto>> Handle(GetRoomTypesQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var roomTypes = await _context.RoomTypes
            .Include(rt => rt.Rooms)
            .Where(rt => rt.HotelId == hotelId.Value && !rt.IsDeleted)
            .OrderBy(rt => rt.Name)
            .ToListAsync(cancellationToken);

        return roomTypes.Select(rt => new RoomTypeDto
        {
            Id = rt.Id,
            HotelId = rt.HotelId,
            Name = rt.Name,
            Description = rt.Description,
            BasePrice = rt.BasePrice,
            MaxCapacity = rt.MaxCapacity,
            TotalRooms = rt.Rooms.Count(r => !r.IsDeleted)
        }).ToList();
    }
}
