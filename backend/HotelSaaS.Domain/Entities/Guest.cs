using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class Guest : TenantEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int TotalVisits { get; set; } = 0;
    public decimal TotalSpending { get; set; } = 0m;
    public DateTime? LastVisitAt { get; set; }
    public string? Preferences { get; set; }
    public bool IsFavorite { get; set; } = false;
    public bool MarketingConsent { get; set; } = false;
    public DateTime? MarketingConsentAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public Hotel? Hotel { get; set; }
    public ICollection<GuestDocument> Documents { get; set; } = new List<GuestDocument>();
    public ICollection<GuestNote> Notes { get; set; } = new List<GuestNote>();
    public ICollection<GuestConsent> Consents { get; set; } = new List<GuestConsent>();
    public ICollection<BookingGuest> BookingGuests { get; set; } = new List<BookingGuest>();
}
