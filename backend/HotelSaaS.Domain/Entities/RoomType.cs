using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class RoomType : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int MaxCapacity { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public Hotel? Hotel { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
