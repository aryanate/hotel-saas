using System.Security.Claims;
using HotelSaaS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HotelSaaS.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? HotelId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var hotelIdClaim = user?.FindFirst("hotel_id")?.Value;
            if (Guid.TryParse(hotelIdClaim, out var hotelId))
            {
                return hotelId;
            }
            return null;
        }
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var subClaim = user?.FindFirst("sub")?.Value 
                           ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(subClaim, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }

    public string Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst("email")?.Value 
                   ?? user?.FindFirst(ClaimTypes.Email)?.Value 
                   ?? string.Empty;
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return Array.Empty<string>();
            }

            return user.Claims
                .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .ToList();
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claimValue = user?.FindFirst("is_super_admin")?.Value;
            return string.Equals(claimValue, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
