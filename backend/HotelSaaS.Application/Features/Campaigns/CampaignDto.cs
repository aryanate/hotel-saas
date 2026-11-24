using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Application.Features.Campaigns;

public class CampaignDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public CampaignStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CampaignRecipientDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid GuestId { get; set; }
    public string? GuestName { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public MessageChannel Channel { get; set; }
    public RecipientStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ExternalMessageId { get; set; }
    public string? ErrorReason { get; set; }
}
