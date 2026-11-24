using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Staff;

public record UpdateStaffCommand : IRequest<StaffDto>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
    }
}

public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, StaffDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateStaffCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StaffDto> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.HotelId == hotelId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException("User", request.Id);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Mobile = request.Mobile;
        user.IsActive = request.IsActive;
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
            Roles = user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList(),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
