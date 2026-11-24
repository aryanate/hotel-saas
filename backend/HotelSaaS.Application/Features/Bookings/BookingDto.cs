using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Application.Features.Bookings;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public Guid GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestMobile { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public BookingSource Source { get; set; }
    public BookingStatus Status { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int TotalNights { get; set; }
    public string? SpecialRequests { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BookingRoomDto> Rooms { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BookingRoomDto
{
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string? RoomTypeName { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}

public class CreateBookingDto
{
    public Guid? GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestMobile { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public List<Guid> RoomIds { get; set; } = new();
    public BookingSource Source { get; set; } = BookingSource.Manual;
    public int NumberOfAdults { get; set; } = 1;
    public int NumberOfChildren { get; set; } = 0;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string? SpecialRequests { get; set; }
}
