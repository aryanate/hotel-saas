using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Billing;

public record GetInvoicePdfQuery(Guid Id) : IRequest<byte[]>;

public class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPdfService _pdfService;

    public GetInvoicePdfQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPdfService pdfService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pdfService = pdfService;
    }

    public async Task<byte[]> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.HotelId == hotelId.Value && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            throw new EntityNotFoundException("Invoice", request.Id);
        }

        return await _pdfService.GenerateInvoicePdfAsync(invoice, cancellationToken);
    }
}

public record SendInvoiceWhatsAppCommand(Guid Id) : IRequest<bool>;

public class SendInvoiceWhatsAppCommandHandler : IRequestHandler<SendInvoiceWhatsAppCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWhatsAppService _whatsAppService;

    public SendInvoiceWhatsAppCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IWhatsAppService whatsAppService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _whatsAppService = whatsAppService;
    }

    public async Task<bool> Handle(SendInvoiceWhatsAppCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.HotelId == hotelId.Value && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            throw new EntityNotFoundException("Invoice", request.Id);
        }

        if (!string.IsNullOrEmpty(invoice.GuestMobile))
        {
            await _whatsAppService.SendMessageAsync(
                invoice.GuestMobile,
                $"Your invoice {invoice.InvoiceNumber} for amount {invoice.TotalAmount:C} is ready.",
                cancellationToken);
        }

        return true;
    }
}

public record SendInvoiceEmailCommand(Guid Id) : IRequest<bool>;

public class SendInvoiceEmailCommandHandler : IRequestHandler<SendInvoiceEmailCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;

    public SendInvoiceEmailCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        IPdfService pdfService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    public async Task<bool> Handle(SendInvoiceEmailCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Include(i => i.Booking)
                .ThenInclude(b => b!.Guest)
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.HotelId == hotelId.Value && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            throw new EntityNotFoundException("Invoice", request.Id);
        }

        var guestEmail = invoice.Booking?.Guest?.Email;
        if (!string.IsNullOrEmpty(guestEmail))
        {
            var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoice, cancellationToken);
            var attachments = new Dictionary<string, byte[]>
            {
                { $"{invoice.InvoiceNumber}.pdf", pdfBytes }
            };

            await _emailService.SendAsync(
                guestEmail,
                $"Invoice {invoice.InvoiceNumber}",
                $"<p>Dear {invoice.GuestName},</p><p>Please find attached your invoice {invoice.InvoiceNumber}.</p>",
                attachments);
        }

        return true;
    }
}

public record GetInvoicePaymentsQuery(Guid Id) : IRequest<IReadOnlyList<PaymentDto>>;

public class GetInvoicePaymentsQueryHandler : IRequestHandler<GetInvoicePaymentsQuery, IReadOnlyList<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInvoicePaymentsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<PaymentDto>> Handle(GetInvoicePaymentsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var payments = await _context.Payments
            .Where(p => p.InvoiceId == request.Id && p.HotelId == hotelId.Value)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        return payments.Select(p => new PaymentDto
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
        }).ToList();
    }
}
