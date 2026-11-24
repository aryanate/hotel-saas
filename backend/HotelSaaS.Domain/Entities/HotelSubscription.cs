using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class HotelSubscription : TenantEntity
{
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndsAt { get; set; }

    // Navigation
    public Hotel? Hotel { get; set; }
    public SubscriptionPlan? Plan { get; set; }
}
