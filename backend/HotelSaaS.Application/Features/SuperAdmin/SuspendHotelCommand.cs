using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.SuperAdmin;

public record SuspendHotelCommand(Guid HotelId) : IRequest<bool>;

public class SuspendHotelCommandHandler : IRequestHandler<SuspendHotelCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SuspendHotelCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(SuspendHotelCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            throw new BusinessRuleViolationException("Access denied. SuperAdmin role required.", "SUPERADMIN_REQUIRED");
        }

        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => h.Id == request.HotelId && !h.IsDeleted, cancellationToken);

        if (hotel == null)
        {
            throw new EntityNotFoundException("Hotel", request.HotelId);
        }

        hotel.IsActive = false;
        hotel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record ActivateHotelCommand(Guid HotelId) : IRequest<bool>;

public class ActivateHotelCommandHandler : IRequestHandler<ActivateHotelCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ActivateHotelCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ActivateHotelCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            throw new BusinessRuleViolationException("Access denied. SuperAdmin role required.", "SUPERADMIN_REQUIRED");
        }

        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => h.Id == request.HotelId && !h.IsDeleted, cancellationToken);

        if (hotel == null)
        {
            throw new EntityNotFoundException("Hotel", request.HotelId);
        }

        hotel.IsActive = true;
        hotel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record GetPlatformStatsQuery : IRequest<PlatformStatsDto>;

public class GetPlatformStatsQueryHandler : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPlatformStatsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            throw new BusinessRuleViolationException("Access denied. SuperAdmin role required.", "SUPERADMIN_REQUIRED");
        }

        var totalHotels = await _context.Hotels.CountAsync(h => !h.IsDeleted, cancellationToken);
        var activeHotels = await _context.Hotels.CountAsync(h => !h.IsDeleted && h.IsActive, cancellationToken);
        var suspendedHotels = totalHotels - activeHotels;

        var totalBookings = await _context.Bookings.CountAsync(b => !b.IsDeleted, cancellationToken);
        var totalRevenue = await _context.Payments.SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
        var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsActive, cancellationToken);
        var totalGuests = await _context.Guests.CountAsync(g => !g.IsDeleted, cancellationToken);

        return new PlatformStatsDto
        {
            TotalHotels = totalHotels,
            ActiveHotels = activeHotels,
            SuspendedHotels = suspendedHotels,
            TotalBookings = totalBookings,
            TotalRevenue = totalRevenue,
            TotalActiveUsers = totalUsers,
            TotalGuests = totalGuests
        };
    }
}
