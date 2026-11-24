using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class Room : AuditableEntity
{
    public string RoomNumber { get; set; } = string.Empty;
    public Guid RoomTypeId { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string Description { get; set; } = string.Empty;
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public Hotel? Hotel { get; set; }
    public RoomType? RoomType { get; set; }
    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
}
