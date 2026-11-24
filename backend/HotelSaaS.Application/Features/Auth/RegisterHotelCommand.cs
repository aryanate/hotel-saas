using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Auth;

public record RegisterHotelCommand : IRequest<AuthResponseDto>
{
    public string HotelName { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PinCode { get; init; } = string.Empty;
    public int NumberOfRooms { get; init; }
}

public class RegisterHotelCommandValidator : AbstractValidator<RegisterHotelCommand>
{
    public RegisterHotelCommandValidator()
    {
        RuleFor(x => x.HotelName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Mobile).NotEmpty().MaximumLength(20);
        RuleFor(x => x.City).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NumberOfRooms).GreaterThan(0);
    }
}

public class RegisterHotelCommandHandler : IRequestHandler<RegisterHotelCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterHotelCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> Handle(RegisterHotelCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new BusinessRuleViolationException("A user with this email already exists.", "EMAIL_ALREADY_EXISTS");
        }

        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = request.HotelName,
            OwnerName = request.OwnerName,
            Email = request.Email,
            Mobile = request.Mobile,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PinCode = request.PinCode,
            NumberOfRooms = request.NumberOfRooms,
            PublicToken = Guid.NewGuid(),
            IsActive = true
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            HotelId = hotel.Id,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.OwnerName,
            LastName = "",
            Mobile = request.Mobile,
            IsActive = true,
            IsSuperAdmin = false
        };

        var adminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "HotelAdmin" && (r.HotelId == null || r.HotelId == hotel.Id), cancellationToken);

        if (adminRole == null)
        {
            adminRole = new Role
            {
                Id = Guid.NewGuid(),
                HotelId = hotel.Id,
                Name = "HotelAdmin",
                Description = "Hotel Administrator with full access",
                IsSystemRole = true
            };
            _context.Roles.Add(adminRole);
        }

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id
        };

        var (refreshTokenStr, refreshExpiry) = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = refreshExpiry
        };

        _context.Hotels.Add(hotel);
        _context.Users.Add(user);
        _context.UserRoles.Add(userRole);
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        var roles = new List<string> { adminRole.Name };
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
            Hotel = new AuthHotelDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Email = hotel.Email,
                City = hotel.City,
                Currency = hotel.Currency,
                PublicToken = hotel.PublicToken
            }
        };
    }
}
