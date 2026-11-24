using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? HotelId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get => CreatedAt; set => CreatedAt = value; }
}
