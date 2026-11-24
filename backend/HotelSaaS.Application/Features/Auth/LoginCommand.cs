using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Auth;

public record LoginCommand : IRequest<AuthResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Hotel)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new BusinessRuleViolationException("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleViolationException("User account is inactive.", "ACCOUNT_INACTIVE");
        }

        if (user.Hotel != null && (!user.Hotel.IsActive || user.Hotel.IsDeleted))
        {
            throw new BusinessRuleViolationException("Hotel account is suspended or inactive.", "HOTEL_INACTIVE");
        }

        user.LastLoginAt = DateTime.UtcNow;

        var roles = user.UserRoles
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role!.Name)
            .ToList();

        if (user.IsSuperAdmin && !roles.Contains("SuperAdmin"))
        {
            roles.Add("SuperAdmin");
        }

        var (refreshTokenStr, refreshExpiry) = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = refreshExpiry
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user, roles);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenStr,
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
