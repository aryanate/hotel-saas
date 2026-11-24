using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class CheckInSession : TenantEntity
{
    public Guid BookingId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public CheckInSessionStatus Status { get; set; } = CheckInSessionStatus.Sent;
    public DateTime? SubmittedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int FailedAttempts { get; set; } = 0;

    // Navigation
    public Booking? Booking { get; set; }
}
