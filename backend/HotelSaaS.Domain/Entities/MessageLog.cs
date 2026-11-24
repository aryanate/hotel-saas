using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class MessageLog : TenantEntity
{
    public string RecipientMobile { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public MessageChannel Channel { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Pending;
    public string? ExternalId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public int RetryCount { get; set; } = 0;
}
