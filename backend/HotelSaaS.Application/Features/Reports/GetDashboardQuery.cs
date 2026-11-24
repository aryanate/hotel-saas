using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Reports;

public record GetDashboardQuery : IRequest<DashboardStatsDto>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var rooms = await _context.Rooms
            .Where(r => r.HotelId == hotelId.Value && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalRooms = rooms.Count;
        var occupiedRooms = rooms.Count(r => r.Status == RoomStatus.Occupied);
        var availableRooms = rooms.Count(r => r.Status == RoomStatus.Available);
        var cleaningRooms = rooms.Count(r => r.Status == RoomStatus.Cleaning);
        var maintenanceRooms = rooms.Count(r => r.Status == RoomStatus.Maintenance);
        var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var todayCheckIns = await _context.Bookings
            .CountAsync(b => b.HotelId == hotelId.Value && !b.IsDeleted &&
                             b.CheckInDate >= today && b.CheckInDate < tomorrow, cancellationToken);

        var todayCheckOuts = await _context.Bookings
            .CountAsync(b => b.HotelId == hotelId.Value && !b.IsDeleted &&
                             b.CheckOutDate >= today && b.CheckOutDate < tomorrow, cancellationToken);

        var todayRevenue = await _context.Payments
            .Where(p => p.HotelId == hotelId.Value && p.PaymentDate >= today && p.PaymentDate < tomorrow)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        var monthlyRevenue = await _context.Payments
            .Where(p => p.HotelId == hotelId.Value && p.PaymentDate >= firstDayOfMonth)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        var pendingPayments = await _context.Invoices
            .Where(i => i.HotelId == hotelId.Value && !i.IsDeleted && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Refunded)
            .SumAsync(i => (decimal?)(i.TotalAmount - i.AmountPaid), cancellationToken) ?? 0m;

        var totalActiveGuests = await _context.Bookings
            .Where(b => b.HotelId == hotelId.Value && !b.IsDeleted && b.Status == BookingStatus.CheckedIn)
            .Select(b => b.GuestId)
            .Distinct()
            .CountAsync(cancellationToken);

        var pendingCheckIns = await _context.CheckInSessions
            .CountAsync(s => s.HotelId == hotelId.Value &&
                             (s.Status == CheckInSessionStatus.Submitted || s.Status == CheckInSessionStatus.Opened), cancellationToken);

        // Weekly revenue (last 7 days)
        var sevenDaysAgo = today.AddDays(-6);
        var recentPayments = await _context.Payments
            .Where(p => p.HotelId == hotelId.Value && p.PaymentDate >= sevenDaysAgo)
            .ToListAsync(cancellationToken);

        var weeklyRevenue = Enumerable.Range(0, 7).Select(offset =>
        {
            var date = sevenDaysAgo.AddDays(offset);
            var dayAmount = recentPayments
                .Where(p => p.PaymentDate.Date == date.Date)
                .Sum(p => p.Amount);
            return new DashboardWeeklyRevenueDto
            {
                Date = date.ToString("ddd"),
                Amount = dayAmount
            };
        }).ToList();

        // Recent Bookings (top 5)
        var recentBookingsDb = await _context.Bookings
            .Include(b => b.Guest)
            .Where(b => b.HotelId == hotelId.Value && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentBookings = recentBookingsDb.Select(b => new DashboardRecentBookingDto
        {
            Id = b.Id.ToString(),
            BookingReference = !string.IsNullOrEmpty(b.BookingReference) ? b.BookingReference : $"BK-{b.Id.ToString().Substring(0, 5).ToUpper()}",
            Guest = new DashboardGuestBriefDto
            {
                FullName = b.Guest?.FullName ?? "Guest",
                Mobile = b.Guest?.Mobile ?? string.Empty
            },
            CheckInDate = b.CheckInDate.ToString("yyyy-MM-dd"),
            CheckOutDate = b.CheckOutDate.ToString("yyyy-MM-dd"),
            Status = b.Status.ToString(),
            TotalNights = Math.Max(1, b.TotalNights)
        }).ToList();

        // Recent Registrations (top 5)
        var recentGuestsDb = await _context.Guests
            .Where(g => g.HotelId == hotelId.Value && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentRegistrations = recentGuestsDb.Select(g => new DashboardRecentGuestDto
        {
            Id = g.Id.ToString(),
            FullName = g.FullName,
            Mobile = g.Mobile,
            Email = g.Email,
            CreatedAt = g.CreatedAt
        }).ToList();

        return new DashboardStatsDto
        {
            TotalRooms = totalRooms,
            OccupiedRooms = occupiedRooms,
            AvailableRooms = availableRooms,
            CleaningRooms = cleaningRooms,
            MaintenanceRooms = maintenanceRooms,
            OccupancyRate = Math.Round(occupancyRate, 2),
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            TodayRevenue = todayRevenue,
            MonthlyRevenue = monthlyRevenue,
            PendingPayments = pendingPayments,
            TotalActiveGuests = totalActiveGuests,
            PendingCheckInSessions = pendingCheckIns,
            WeeklyRevenue = weeklyRevenue,
            RecentBookings = recentBookings,
            RecentRegistrations = recentRegistrations
        };
    }
}
