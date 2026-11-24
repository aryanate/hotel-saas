using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class BookingRoom : TenantEntity
{
    public Guid BookingId { get; set; }
    public Guid RoomId { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    // Navigation
    public Booking? Booking { get; set; }
    public Room? Room { get; set; }
}
