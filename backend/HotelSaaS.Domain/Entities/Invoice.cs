using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class Invoice : AuditableEntity
{
    public Guid BookingId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestMobile { get; set; } = string.Empty;
    public string? GuestGstin { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0m;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; } = 0m;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public string? Notes { get; set; }
    public string? StorageKey { get; set; }
    public bool IsDeleted { get; set; } = false;

    public decimal OutstandingBalance => TotalAmount - AmountPaid;

    // Navigation
    public Booking? Booking { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
