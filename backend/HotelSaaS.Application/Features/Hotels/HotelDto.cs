namespace HotelSaaS.Application.Features.Hotels;

public class HotelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public int NumberOfRooms { get; set; }
    public Guid PublicToken { get; set; }
    public string? LogoUrl { get; set; }
    public string? GstIn { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public int InvoiceSequence { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public string Currency { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public bool IsActive { get; set; }
}

public class UpdateHotelDto
{
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? GstIn { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public string Currency { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
}
