using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Staff;

public record InviteStaffCommand : IRequest<StaffDto>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string InitialPassword { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
}

public class InviteStaffCommandValidator : AbstractValidator<InviteStaffCommand>
{
    public InviteStaffCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InitialPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Roles).NotEmpty().WithMessage("At least one role must be assigned.");
    }
}

public class InviteStaffCommandHandler : IRequestHandler<InviteStaffCommand, StaffDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public InviteStaffCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<StaffDto> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var exists = await _context.Users
            .AnyAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleViolationException("A user with this email already exists.", "USER_EXISTS");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Mobile = request.Mobile,
            PasswordHash = _passwordHasher.HashPassword(request.InitialPassword),
            IsActive = true
        };

        var availableRoles = await _context.Roles
            .Where(r => (r.HotelId == hotelId.Value || r.HotelId == null) && request.Roles.Contains(r.Name))
            .ToListAsync(cancellationToken);

        foreach (var role in availableRoles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new StaffDto
        {
            Id = user.Id,
            HotelId = user.HotelId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Mobile = user.Mobile,
            IsActive = user.IsActive,
            Roles = availableRoles.Select(r => r.Name).ToList(),
            CreatedAt = user.CreatedAt
        };
    }
}
