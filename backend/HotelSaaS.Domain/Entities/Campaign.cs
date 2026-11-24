using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class Campaign : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public Hotel? Hotel { get; set; }
    public ICollection<CampaignRecipient> Recipients { get; set; } = new List<CampaignRecipient>();
}
