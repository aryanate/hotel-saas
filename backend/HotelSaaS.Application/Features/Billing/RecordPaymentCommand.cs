using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Billing;

public record RecordPaymentCommand : IRequest<PaymentDto>
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public PaymentMethod PaymentMethod { get; init; } = PaymentMethod.Cash;
    public string? Reference { get; init; }
    public string? Notes { get; init; }
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, PaymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RecordPaymentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaymentDto> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Booking)
                .ThenInclude(b => b!.Guest)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.HotelId == hotelId.Value && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            throw new EntityNotFoundException("Invoice", request.InvoiceId);
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new BusinessRuleViolationException("Invoice is already fully paid.", "INVOICE_ALREADY_PAID");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            InvoiceId = invoice.Id,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentDate = DateTime.UtcNow,
            Reference = request.Reference,
            Notes = request.Notes,
            RecordedBy = _currentUserService.UserId
        };

        invoice.AmountPaid += request.Amount;
        if (invoice.AmountPaid >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.AmountPaid > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        invoice.UpdatedBy = _currentUserService.UserId;
        invoice.UpdatedAt = DateTime.UtcNow;

        if (invoice.Booking?.Guest != null)
        {
            invoice.Booking.Guest.TotalSpending += request.Amount;
            invoice.Booking.Guest.UpdatedBy = _currentUserService.UserId;
            invoice.Booking.Guest.UpdatedAt = DateTime.UtcNow;
        }

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return new PaymentDto
        {
            Id = payment.Id,
            InvoiceId = payment.InvoiceId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentDate = payment.PaymentDate,
            Reference = payment.Reference,
            Notes = payment.Notes,
            RecordedBy = payment.RecordedBy,
            CreatedAt = payment.CreatedAt
        };
    }
}
