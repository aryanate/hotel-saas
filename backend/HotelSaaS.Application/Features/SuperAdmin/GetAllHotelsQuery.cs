using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.SuperAdmin;

public record GetAllHotelsQuery : IRequest<IReadOnlyList<AdminHotelDto>>;

public class GetAllHotelsQueryHandler : IRequestHandler<GetAllHotelsQuery, IReadOnlyList<AdminHotelDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAllHotelsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<AdminHotelDto>> Handle(GetAllHotelsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            throw new BusinessRuleViolationException("Access denied. SuperAdmin role required.", "SUPERADMIN_REQUIRED");
        }

        var hotels = await _context.Hotels
            .Include(h => h.Subscriptions)
                .ThenInclude(s => s.Plan)
            .Where(h => !h.IsDeleted)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        var hotelIds = hotels.Select(h => h.Id).ToList();

        var bookingCounts = await _context.Bookings
            .Where(b => hotelIds.Contains(b.HotelId) && !b.IsDeleted)
            .GroupBy(b => b.HotelId)
            .Select(g => new { HotelId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.HotelId, x => x.Count, cancellationToken);

        var revenues = await _context.Invoices
            .Where(i => hotelIds.Contains(i.HotelId) && !i.IsDeleted)
            .GroupBy(i => i.HotelId)
            .Select(g => new { HotelId = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
            .ToDictionaryAsync(x => x.HotelId, x => x.Revenue, cancellationToken);

        return hotels.Select(h => new AdminHotelDto
        {
            Id = h.Id,
            Name = h.Name,
            OwnerName = h.OwnerName,
            Email = h.Email,
            Mobile = h.Mobile,
            City = h.City,
            State = h.State,
            NumberOfRooms = h.NumberOfRooms,
            IsActive = h.IsActive,
            CreatedAt = h.CreatedAt,
            TotalBookings = bookingCounts.GetValueOrDefault(h.Id, 0),
            TotalRevenue = revenues.GetValueOrDefault(h.Id, 0m),
            CurrentPlanName = h.Subscriptions.OrderByDescending(s => s.StartDate).FirstOrDefault()?.Plan?.Name
        }).ToList();
    }
}

public record GetHotelByIdAdminQuery(Guid Id) : IRequest<AdminHotelDto>;

public class GetHotelByIdAdminQueryHandler : IRequestHandler<GetHotelByIdAdminQuery, AdminHotelDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetHotelByIdAdminQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AdminHotelDto> Handle(GetHotelByIdAdminQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            throw new BusinessRuleViolationException("Access denied. SuperAdmin role required.", "SUPERADMIN_REQUIRED");
        }

        var h = await _context.Hotels
            .Include(h => h.Subscriptions)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(h => h.Id == request.Id && !h.IsDeleted, cancellationToken);

        if (h == null)
        {
            throw new EntityNotFoundException("Hotel", request.Id);
        }

        var bookingCount = await _context.Bookings
            .CountAsync(b => b.HotelId == h.Id && !b.IsDeleted, cancellationToken);

        var revenue = await _context.Invoices
            .Where(i => i.HotelId == h.Id && !i.IsDeleted)
            .SumAsync(i => (decimal?)i.TotalAmount, cancellationToken) ?? 0m;

        return new AdminHotelDto
        {
            Id = h.Id,
            Name = h.Name,
            OwnerName = h.OwnerName,
            Email = h.Email,
            Mobile = h.Mobile,
            City = h.City,
            State = h.State,
            NumberOfRooms = h.NumberOfRooms,
            IsActive = h.IsActive,
            CreatedAt = h.CreatedAt,
            TotalBookings = bookingCount,
            TotalRevenue = revenue,
            CurrentPlanName = h.Subscriptions.OrderByDescending(s => s.StartDate).FirstOrDefault()?.Plan?.Name
        };
    }
}
