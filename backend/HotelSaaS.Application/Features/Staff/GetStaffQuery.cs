using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Staff;

public record GetStaffQuery : IRequest<IReadOnlyList<StaffDto>>;

public class GetStaffQueryHandler : IRequestHandler<GetStaffQuery, IReadOnlyList<StaffDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStaffQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<StaffDto>> Handle(GetStaffQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var staff = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.HotelId == hotelId.Value && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        return staff.Select(u => new StaffDto
        {
            Id = u.Id,
            HotelId = u.HotelId,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Mobile = u.Mobile,
            IsActive = u.IsActive,
            Roles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList(),
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt
        }).ToList();
    }
}

public record GetStaffRolesQuery(Guid StaffId) : IRequest<IReadOnlyList<StaffRoleDto>>;

public class GetStaffRolesQueryHandler : IRequestHandler<GetStaffRolesQuery, IReadOnlyList<StaffRoleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStaffRolesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<StaffRoleDto>> Handle(GetStaffRolesQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == request.StaffId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var roles = await _context.Roles
            .Where(r => r.HotelId == hotelId.Value || r.HotelId == null)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new StaffRoleDto
        {
            RoleId = r.Id,
            RoleName = r.Name,
            Description = r.Description,
            IsAssigned = userRoles.Contains(r.Id)
        }).ToList();
    }
}
