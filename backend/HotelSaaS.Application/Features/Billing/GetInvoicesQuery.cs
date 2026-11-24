using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Application.Common.Models;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Billing;

public record GetInvoicesQuery : IRequest<PagedResult<InvoiceDto>>
{
    public InvoiceStatus? Status { get; init; }
    public string? Search { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInvoicesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var query = _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.HotelId == hotelId.Value && !i.IsDeleted);

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(i => i.InvoiceNumber.Contains(request.Search) ||
                                     i.GuestName.Contains(request.Search) ||
                                     i.GuestMobile.Contains(request.Search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = invoices.Select(i => new InvoiceDto
        {
            Id = i.Id,
            HotelId = i.HotelId,
            BookingId = i.BookingId,
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            GuestName = i.GuestName,
            GuestMobile = i.GuestMobile,
            GuestGstin = i.GuestGstin,
            SubTotal = i.SubTotal,
            TaxAmount = i.TaxAmount,
            DiscountAmount = i.DiscountAmount,
            TotalAmount = i.TotalAmount,
            AmountPaid = i.AmountPaid,
            Status = i.Status,
            Notes = i.Notes,
            CreatedAt = i.CreatedAt,
            Items = i.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                Description = item.Description,
                Category = item.Category,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList(),
            Payments = i.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                InvoiceId = p.InvoiceId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentDate = p.PaymentDate,
                Reference = p.Reference,
                Notes = p.Notes,
                RecordedBy = p.RecordedBy,
                CreatedAt = p.CreatedAt
            }).ToList()
        }).ToList();

        return new PagedResult<InvoiceDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
