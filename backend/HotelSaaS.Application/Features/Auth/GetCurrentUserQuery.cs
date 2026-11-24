using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Auth;

public class CurrentUserResponseDto
{
    public AuthUserDto User { get; set; } = new();
    public AuthHotelDto? Hotel { get; set; }
}

public record GetCurrentUserQuery : IRequest<CurrentUserResponseDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CurrentUserResponseDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException(nameof(User), userId);
        }

        AuthHotelDto? hotelDto = null;
        if (user.HotelId.HasValue)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == user.HotelId.Value && !h.IsDeleted, cancellationToken);

            if (hotel != null)
            {
                hotelDto = new AuthHotelDto
                {
                    Id = hotel.Id,
                    Name = hotel.Name,
                    Email = hotel.Email,
                    City = hotel.City,
                    Currency = hotel.Currency,
                    PublicToken = hotel.PublicToken
                };
            }
        }

        var roles = user.UserRoles
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role!.Name)
            .ToList();

        if (user.IsSuperAdmin && !roles.Contains("SuperAdmin"))
        {
            roles.Add("SuperAdmin");
        }

        return new CurrentUserResponseDto
        {
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
            Hotel = hotelDto
        };
    }
}

