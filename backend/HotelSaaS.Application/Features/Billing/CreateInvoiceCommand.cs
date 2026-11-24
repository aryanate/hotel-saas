using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Billing;

public record CreateInvoiceItemRequest
{
    public string Description { get; init; } = string.Empty;
    public ItemCategory Category { get; init; } = ItemCategory.Room;
    public decimal Quantity { get; init; } = 1;
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }
}

public record CreateInvoiceCommand : IRequest<InvoiceDto>
{
    public Guid BookingId { get; init; }
    public List<CreateInvoiceItemRequest> Items { get; init; } = new();
    public decimal DiscountAmount { get; init; } = 0;
    public string? GuestGstin { get; init; }
    public string? Notes { get; init; }
}

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Invoice must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.TaxRate).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateInvoiceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => h.Id == hotelId.Value && !h.IsDeleted, cancellationToken);

        if (hotel == null)
        {
            throw new EntityNotFoundException("Hotel", hotelId.Value);
        }

        var booking = await _context.Bookings
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.HotelId == hotelId.Value && !b.IsDeleted, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException("Booking", request.BookingId);
        }

        hotel.InvoiceSequence++;
        var invoiceNumber = $"{hotel.InvoicePrefix}-{hotel.InvoiceSequence:D5}";

        decimal subTotal = 0;
        decimal totalTax = 0;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            BookingId = booking.Id,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.UtcNow,
            GuestName = booking.Guest?.FullName ?? "Guest",
            GuestMobile = booking.Guest?.Mobile ?? "",
            GuestGstin = request.GuestGstin,
            DiscountAmount = request.DiscountAmount,
            Status = InvoiceStatus.Unpaid,
            Notes = request.Notes,
            CreatedBy = _currentUserService.UserId,
            UpdatedBy = _currentUserService.UserId
        };

        foreach (var itemReq in request.Items)
        {
            var itemTotal = itemReq.Quantity * itemReq.UnitPrice;
            var itemTax = itemTotal * (itemReq.TaxRate / 100m);

            subTotal += itemTotal;
            totalTax += itemTax;

            invoice.Items.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId.Value,
                InvoiceId = invoice.Id,
                Description = itemReq.Description,
                Category = itemReq.Category,
                Quantity = itemReq.Quantity,
                UnitPrice = itemReq.UnitPrice,
                TaxRate = itemReq.TaxRate,
                TaxAmount = itemTax,
                TotalAmount = itemTotal + itemTax
            });
        }

        invoice.SubTotal = subTotal;
        invoice.TaxAmount = totalTax;
        invoice.TotalAmount = Math.Max(0, subTotal + totalTax - request.DiscountAmount);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return new InvoiceDto
        {
            Id = invoice.Id,
            HotelId = invoice.HotelId,
            BookingId = invoice.BookingId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            GuestName = invoice.GuestName,
            GuestMobile = invoice.GuestMobile,
            GuestGstin = invoice.GuestGstin,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            AmountPaid = invoice.AmountPaid,
            Status = invoice.Status,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                Description = item.Description,
                Category = item.Category,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }
}
