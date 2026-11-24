namespace HotelSaaS.Application.Features.SuperAdmin;

public class PlatformStatsDto
{
    public int TotalHotels { get; set; }
    public int ActiveHotels { get; set; }
    public int SuspendedHotels { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalActiveUsers { get; set; }
    public int TotalGuests { get; set; }
}

public class AdminHotelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int NumberOfRooms { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public string? CurrentPlanName { get; set; }
}

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public int MaxBookingsPerMonth { get; set; }
    public string FeaturesJson { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
