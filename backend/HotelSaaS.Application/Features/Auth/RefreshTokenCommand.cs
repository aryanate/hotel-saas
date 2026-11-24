using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Auth;

public record RefreshTokenCommand : IRequest<AuthResponseDto>
{
    public string RefreshToken { get; init; } = string.Empty;
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u!.Hotel)
            .Include(rt => rt.User)
                .ThenInclude(u => u!.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingToken == null || existingToken.IsRevoked || existingToken.User == null)
        {
            throw new BusinessRuleViolationException("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");
        }

        var user = existingToken.User;
        if (!user.IsActive || user.IsDeleted)
        {
            throw new BusinessRuleViolationException("User account is inactive.", "ACCOUNT_INACTIVE");
        }

        existingToken.RevokedAt = DateTime.UtcNow;

        var (newRefreshTokenStr, refreshExpiry) = _tokenService.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenStr,
            ExpiresAt = refreshExpiry
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role!.Name)
            .ToList();

        if (user.IsSuperAdmin && !roles.Contains("SuperAdmin"))
        {
            roles.Add("SuperAdmin");
        }

        var accessToken = _tokenService.GenerateAccessToken(user, roles);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenStr,
            ExpiresAt = refreshExpiry,
            User = new AuthUserDto
            {
                Id = user.Id,
                HotelId = user.HotelId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mobile = user.Mobile,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = roles
            },
            Hotel = user.Hotel != null ? new AuthHotelDto
            {
                Id = user.Hotel.Id,
                Name = user.Hotel.Name,
                Email = user.Hotel.Email,
                City = user.Hotel.City,
                Currency = user.Hotel.Currency,
                PublicToken = user.Hotel.PublicToken
            } : null
        };
    }
}
