using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class GuestConsent : TenantEntity
{
    public Guid GuestId { get; set; }
    public ConsentType ConsentType { get; set; }
    public string ConsentText { get; set; } = string.Empty;
    public string ConsentVersion { get; set; } = string.Empty;
    public DateTime ConsentGivenAt { get; set; }
    public bool IsWithdrawn { get; set; } = false;
    public DateTime? WithdrawnAt { get; set; }

    // Aliases for compatibility
    public bool IsGranted { get => !IsWithdrawn; set => IsWithdrawn = !value; }
    public DateTime GrantedAt { get => ConsentGivenAt; set => ConsentGivenAt = value; }

    // Navigation
    public Guest? Guest { get; set; }
}
