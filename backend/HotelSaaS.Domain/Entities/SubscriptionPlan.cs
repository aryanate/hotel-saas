using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public int MaxBookingsPerMonth { get; set; }
    public string FeaturesJson { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<HotelSubscription> HotelSubscriptions { get; set; } = new List<HotelSubscription>();
}
