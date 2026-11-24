using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class Hotel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public int NumberOfRooms { get; set; }
    public Guid PublicToken { get; set; } = Guid.NewGuid();
    public string? LogoUrl { get; set; }
    public string? GstIn { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public int InvoiceSequence { get; set; } = 0;
    public TimeOnly CheckInTime { get; set; } = new TimeOnly(14, 0);
    public TimeOnly CheckOutTime { get; set; } = new TimeOnly(11, 0);
    public string Currency { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Guest> Guests { get; set; } = new List<Guest>();
    public ICollection<HotelSubscription> Subscriptions { get; set; } = new List<HotelSubscription>();
}
