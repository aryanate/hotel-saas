using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class CampaignRecipient : TenantEntity
{
    public Guid CampaignId { get; set; }
    public Guid GuestId { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? PersonalizedMessage { get; set; }
    public RecipientStatus Status { get; set; } = RecipientStatus.Pending;
    public MessageChannel Channel { get; set; } = MessageChannel.WhatsApp;
    public string? ExternalMessageId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorReason { get => ErrorMessage; set => ErrorMessage = value; }

    // Navigation
    public Campaign? Campaign { get; set; }
    public Guest? Guest { get; set; }
}
