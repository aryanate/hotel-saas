using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Interfaces;
using AppInterfaces = HotelSaaS.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelSaaS.Infrastructure.Services;

public class JwtService : HotelSaaS.Domain.Interfaces.ITokenService, AppInterfaces.ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secret = _configuration["Jwt:Secret"] ?? "DefaultSecretKeyThatIsAtLeast32BytesLongForTesting123!";
        _issuer = _configuration["Jwt:Issuer"] ?? "HotelSaaS";
        _audience = _configuration["Jwt:Audience"] ?? "HotelSaaSClients";

        if (!int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out _accessTokenExpiryMinutes))
        {
            _accessTokenExpiryMinutes = 15;
        }

        if (!int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out _refreshTokenExpiryDays))
        {
            _refreshTokenExpiryDays = 7;
        }
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("is_super_admin", user.IsSuperAdmin ? "true" : "false")
        };

        if (user.HotelId.HasValue)
        {
            claims.Add(new Claim("hotel_id", user.HotelId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_accessTokenExpiryMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public (string Token, DateTime Expiry) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var token = Convert.ToBase64String(randomBytes);
        var expiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        return (token, expiry);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = !string.IsNullOrEmpty(_issuer),
            ValidIssuer = _issuer,
            ValidateAudience = !string.IsNullOrEmpty(_audience),
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
