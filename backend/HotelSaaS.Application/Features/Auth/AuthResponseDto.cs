namespace HotelSaaS.Application.Features.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthUserDto User { get; set; } = new();
    public AuthHotelDto? Hotel { get; set; }
}

public class AuthUserDto
{
    public Guid Id { get; set; }
    public Guid? HotelId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class AuthHotelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
    public Guid PublicToken { get; set; }
}
