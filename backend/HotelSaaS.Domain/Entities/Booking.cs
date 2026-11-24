using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class Booking : AuditableEntity
{
    public Guid GuestId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public BookingSource Source { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public int NumberOfAdults { get; set; } = 1;
    public int NumberOfChildren { get; set; } = 0;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string? SpecialRequests { get; set; }
    public bool IsDeleted { get; set; } = false;

    public int TotalNights => (CheckOutDate - CheckInDate).Days;

    // Navigation
    public Guest? Guest { get; set; }
    public Hotel? Hotel { get; set; }
    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    public ICollection<BookingGuest> BookingGuests { get; set; } = new List<BookingGuest>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<CheckInSession> CheckInSessions { get; set; } = new List<CheckInSession>();
}
