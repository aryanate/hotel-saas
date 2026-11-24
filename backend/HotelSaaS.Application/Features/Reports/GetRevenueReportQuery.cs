using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Reports;

public record GetRevenueReportQuery : IRequest<RevenueReportDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetRevenueReportQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var from = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = request.ToDate ?? DateTime.UtcNow;

        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Where(i => i.HotelId == hotelId.Value && !i.IsDeleted &&
                        i.InvoiceDate >= from && i.InvoiceDate <= to)
            .ToListAsync(cancellationToken);

        var totalRevenue = invoices.Sum(i => i.TotalAmount);
        var totalTax = invoices.Sum(i => i.TaxAmount);
        var totalDiscounts = invoices.Sum(i => i.DiscountAmount);

        var roomRevenue = invoices.SelectMany(i => i.Items)
            .Where(item => item.Category == ItemCategory.Room)
            .Sum(item => item.TotalAmount);

        var otherRevenue = invoices.SelectMany(i => i.Items)
            .Where(item => item.Category != ItemCategory.Room)
            .Sum(item => item.TotalAmount);

        var daily = invoices
            .GroupBy(i => i.InvoiceDate.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Amount = g.Sum(i => i.TotalAmount),
                InvoiceCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new RevenueReportDto
        {
            TotalRevenue = totalRevenue,
            RoomRevenue = roomRevenue,
            OtherRevenue = otherRevenue,
            TotalTax = totalTax,
            TotalDiscounts = totalDiscounts,
            DailyBreakdown = daily
        };
    }
}
