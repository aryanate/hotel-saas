using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Application.Features.Billing;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public Guid BookingId { get; set; }
    public string? BookingReference { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestMobile { get; set; } = string.Empty;
    public string? GuestGstin { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingBalance => TotalAmount - AmountPaid;
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Guid RecordedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
