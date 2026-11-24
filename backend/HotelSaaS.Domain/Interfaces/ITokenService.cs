using System.Security.Claims;
using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Domain.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    (string Token, DateTime Expiry) GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
