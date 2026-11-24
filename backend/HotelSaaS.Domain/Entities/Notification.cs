using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    // Navigation
    public User? User { get; set; }
}
