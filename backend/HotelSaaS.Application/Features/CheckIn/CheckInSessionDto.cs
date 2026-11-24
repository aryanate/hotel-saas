using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Application.Features.CheckIn;

public class CheckInSessionDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public Guid BookingId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public CheckInSessionStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? GuestName { get; set; }
    public string? GuestMobile { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestAddress { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentUrl { get; set; }
    public List<string> DocumentUrls { get; set; } = new();
    public string? BookingReference { get; set; }
    public string? HotelName { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public List<string> RoomNumbers { get; set; } = new();
}

public class PublicCheckInInfoDto
{
    public string HotelName { get; set; } = string.Empty;
    public string? HotelLogoUrl { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestMobile { get; set; } = string.Empty;
    public string BookingReference { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public CheckInSessionStatus Status { get; set; }
    public bool IsExpired { get; set; }
}
