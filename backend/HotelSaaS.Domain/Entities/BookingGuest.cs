using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class BookingGuest : TenantEntity
{
    public Guid BookingId { get; set; }
    public Guid GuestId { get; set; }
    public bool IsPrimary { get; set; } = false;

    // Navigation
    public Booking? Booking { get; set; }
    public Guest? Guest { get; set; }
}
