using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Staff;

public record RemoveStaffCommand(Guid Id) : IRequest<bool>;

public class RemoveStaffCommandHandler : IRequestHandler<RemoveStaffCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveStaffCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(RemoveStaffCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        if (request.Id == _currentUserService.UserId)
        {
            throw new BusinessRuleViolationException("You cannot remove your own account.", "CANNOT_REMOVE_SELF");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.HotelId == hotelId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException("User", request.Id);
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record AssignRolesCommand(Guid StaffId, List<string> Roles) : IRequest<StaffDto>;

public class AssignRolesCommandHandler : IRequestHandler<AssignRolesCommand, StaffDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssignRolesCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StaffDto> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.StaffId && u.HotelId == hotelId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException("User", request.StaffId);
        }

        var targetRoles = await _context.Roles
            .Where(r => (r.HotelId == hotelId.Value || r.HotelId == null) && request.Roles.Contains(r.Name))
            .ToListAsync(cancellationToken);

        // Remove old roles
        _context.UserRoles.RemoveRange(user.UserRoles);

        // Add new roles
        foreach (var r in targetRoles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = r.Id
            });
        }

        user.UpdatedAt = DateTime.UtcNow;
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
            Roles = targetRoles.Select(r => r.Name).ToList(),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
