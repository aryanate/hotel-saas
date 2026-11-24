using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class FavoriteGuest : TenantEntity
{
    public Guid GuestId { get; set; }
    public Guid MarkedBy { get; set; }
    public DateTime MarkedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Guest? Guest { get; set; }
}
