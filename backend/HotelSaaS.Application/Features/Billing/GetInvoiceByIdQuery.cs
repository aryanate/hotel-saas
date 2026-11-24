using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Billing;

public record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInvoiceByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var i = await _context.Invoices
            .Include(i => i.Booking)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.HotelId == hotelId.Value && !i.IsDeleted, cancellationToken);

        if (i == null)
        {
            throw new EntityNotFoundException("Invoice", request.Id);
        }

        return new InvoiceDto
        {
            Id = i.Id,
            HotelId = i.HotelId,
            BookingId = i.BookingId,
            BookingReference = i.Booking?.BookingReference,
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
        };
    }
}

public record DownloadInvoicePdfQuery(Guid Id) : IRequest<byte[]>;

public class DownloadInvoicePdfQueryHandler : IRequestHandler<DownloadInvoicePdfQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPdfService _pdfService;

    public DownloadInvoicePdfQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPdfService pdfService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pdfService = pdfService;
    }

    public async Task<byte[]> Handle(DownloadInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.HotelId == hotelId.Value && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            throw new EntityNotFoundException("Invoice", request.Id);
        }

        return await _pdfService.GenerateInvoicePdfAsync(invoice, cancellationToken);
    }
}
