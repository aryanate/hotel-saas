using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Reports;

public record GetGstReportQuery(int Month, int Year) : IRequest<GstReportDto>;

public class GetGstReportQueryHandler : IRequestHandler<GetGstReportQuery, GstReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGstReportQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GstReportDto> Handle(GetGstReportQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => h.Id == hotelId.Value && !h.IsDeleted, cancellationToken);

        var startDate = new DateTime(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1);

        var invoices = await _context.Invoices
            .Where(i => i.HotelId == hotelId.Value && !i.IsDeleted &&
                        i.InvoiceDate >= startDate && i.InvoiceDate < endDate)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);

        var totalTaxable = invoices.Sum(i => i.SubTotal - i.DiscountAmount);
        var totalTax = invoices.Sum(i => i.TaxAmount);
        var halfTax = Math.Round(totalTax / 2, 2);

        var invoiceSummaries = invoices.Select(i => new GstInvoiceSummaryDto
        {
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            GuestName = i.GuestName,
            GuestGstin = i.GuestGstin,
            TaxableAmount = i.SubTotal - i.DiscountAmount,
            TaxAmount = i.TaxAmount,
            TotalAmount = i.TotalAmount
        }).ToList();

        return new GstReportDto
        {
            HotelGstIn = hotel?.GstIn ?? "",
            Month = request.Month,
            Year = request.Year,
            TotalTaxableAmount = totalTaxable,
            TotalCgst = halfTax,
            TotalSgst = halfTax,
            TotalIgst = 0m,
            TotalTax = totalTax,
            TotalInvoices = invoices.Count,
            Invoices = invoiceSummaries
        };
    }
}

public record GetGuestReportQuery : IRequest<GuestReportDto>;

public class GetGuestReportQueryHandler : IRequestHandler<GetGuestReportQuery, GuestReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGuestReportQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GuestReportDto> Handle(GetGuestReportQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var guests = await _context.Guests
            .Where(g => g.HotelId == hotelId.Value && !g.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalGuests = guests.Count;
        var repeatGuests = guests.Count(g => g.TotalVisits > 1);
        var newGuests = guests.Count(g => g.TotalVisits <= 1);
        var avgSpending = totalGuests > 0 ? guests.Average(g => g.TotalSpending) : 0;
        var favCount = guests.Count(g => g.IsFavorite);

        return new GuestReportDto
        {
            TotalGuests = totalGuests,
            RepeatGuests = repeatGuests,
            NewGuests = newGuests,
            AverageSpendingPerGuest = Math.Round(avgSpending, 2),
            FavoriteGuestsCount = favCount
        };
    }
}

public record GetInvoiceReportQuery : IRequest<InvoiceReportDto>;

public class GetInvoiceReportQueryHandler : IRequestHandler<GetInvoiceReportQuery, InvoiceReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInvoiceReportQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<InvoiceReportDto> Handle(GetInvoiceReportQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var invoices = await _context.Invoices
            .Where(i => i.HotelId == hotelId.Value && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalBilled = invoices.Sum(i => i.TotalAmount);
        var totalCollected = invoices.Sum(i => i.AmountPaid);

        return new InvoiceReportDto
        {
            TotalInvoices = invoices.Count,
            TotalBilled = totalBilled,
            TotalCollected = totalCollected,
            TotalOutstanding = totalBilled - totalCollected,
            PaidInvoicesCount = invoices.Count(i => i.Status == Domain.Enums.InvoiceStatus.Paid),
            UnpaidInvoicesCount = invoices.Count(i => i.Status == Domain.Enums.InvoiceStatus.Unpaid || i.Status == Domain.Enums.InvoiceStatus.PartiallyPaid)
        };
    }
}
