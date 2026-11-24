using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Application.Features.Rooms;

public class RoomDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public Guid RoomTypeId { get; set; }
    public string? RoomTypeName { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string Description { get; set; } = string.Empty;
    public RoomStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RoomTypeDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int MaxCapacity { get; set; }
    public int TotalRooms { get; set; }
}
