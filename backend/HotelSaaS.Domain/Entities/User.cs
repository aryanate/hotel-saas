using HotelSaaS.Domain.Common;

namespace HotelSaaS.Domain.Entities;

public class User : BaseEntity
{
    public Guid? HotelId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsSuperAdmin { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public Hotel? Hotel { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
