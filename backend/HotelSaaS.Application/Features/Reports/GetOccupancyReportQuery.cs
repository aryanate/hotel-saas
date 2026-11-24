using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Reports;

public record GetOccupancyReportQuery : IRequest<OccupancyReportDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class GetOccupancyReportQueryHandler : IRequestHandler<GetOccupancyReportQuery, OccupancyReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetOccupancyReportQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<OccupancyReportDto> Handle(GetOccupancyReportQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var from = request.FromDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var to = request.ToDate?.Date ?? DateTime.UtcNow.Date;

        var totalRooms = await _context.Rooms
            .CountAsync(r => r.HotelId == hotelId.Value && !r.IsDeleted, cancellationToken);

        var bookingRooms = await _context.BookingRooms
            .Include(br => br.Booking)
            .Where(br => br.HotelId == hotelId.Value &&
                         br.Booking != null &&
                         !br.Booking.IsDeleted &&
                         br.Booking.Status != BookingStatus.Cancelled &&
                         br.CheckInDate < to.AddDays(1) && br.CheckOutDate > from)
            .ToListAsync(cancellationToken);

        var dailyBreakdown = new List<DailyOccupancyDto>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            var occupied = bookingRooms.Count(br => br.CheckInDate < nextDate && br.CheckOutDate > date);
            var rate = totalRooms > 0 ? Math.Round((double)occupied / totalRooms * 100, 2) : 0;

            dailyBreakdown.Add(new DailyOccupancyDto
            {
                Date = date,
                OccupiedRooms = occupied,
                OccupancyRate = rate
            });
        }

        var avgRate = dailyBreakdown.Any() ? Math.Round(dailyBreakdown.Average(d => d.OccupancyRate), 2) : 0;

        return new OccupancyReportDto
        {
            TotalRooms = totalRooms,
            AverageOccupancyRate = avgRate,
            DailyBreakdown = dailyBreakdown
        };
    }
}
