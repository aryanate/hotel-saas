using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class GuestNote : TenantEntity
{
    public Guid GuestId { get; set; }
    public string Note { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }

    // Navigation
    public Guest? Guest { get; set; }
}
