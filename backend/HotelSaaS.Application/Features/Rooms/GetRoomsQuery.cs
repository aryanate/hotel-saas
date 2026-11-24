using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record GetRoomsQuery : IRequest<IReadOnlyList<RoomDto>>
{
    public RoomStatus? Status { get; init; }
    public Guid? RoomTypeId { get; init; }
    public int? Floor { get; init; }
    public string? Search { get; init; }
}

public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, IReadOnlyList<RoomDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetRoomsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var query = _context.Rooms
            .Include(r => r.RoomType)
            .Where(r => r.HotelId == hotelId.Value && !r.IsDeleted);

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.RoomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == request.RoomTypeId.Value);
        }

        if (request.Floor.HasValue)
        {
            query = query.Where(r => r.Floor == request.Floor.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(r => r.RoomNumber.Contains(request.Search));
        }

        var rooms = await query
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync(cancellationToken);

        return rooms.Select(r => new RoomDto
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
