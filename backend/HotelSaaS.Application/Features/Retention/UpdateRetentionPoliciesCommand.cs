using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Retention;

public record UpdateRetentionPoliciesCommand(List<UpdateRetentionPolicyRequest> Policies) : IRequest<IReadOnlyList<RetentionPolicyDto>>;

public class UpdateRetentionPoliciesCommandHandler : IRequestHandler<UpdateRetentionPoliciesCommand, IReadOnlyList<RetentionPolicyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRetentionPoliciesCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RetentionPolicyDto>> Handle(UpdateRetentionPoliciesCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var existingPolicies = await _context.RetentionPolicies
            .Where(p => p.HotelId == hotelId.Value)
            .ToListAsync(cancellationToken);

        foreach (var req in request.Policies)
        {
            var existing = existingPolicies.FirstOrDefault(p => p.RecordType == req.RecordType);
            if (existing != null)
            {
                existing.RetentionDays = req.RetentionDays;
                existing.IsLegalHold = req.IsLegalHold;
                existing.Notes = req.Notes;
                existing.UpdatedBy = _currentUserService.UserId;
            }
            else
            {
                _context.RetentionPolicies.Add(new RetentionPolicy
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId.Value,
                    RecordType = req.RecordType,
                    RetentionDays = req.RetentionDays,
                    IsLegalHold = req.IsLegalHold,
                    Notes = req.Notes,
                    UpdatedBy = _currentUserService.UserId
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updated = await _context.RetentionPolicies
            .Where(p => p.HotelId == hotelId.Value)
            .ToListAsync(cancellationToken);

        return updated.Select(p => new RetentionPolicyDto
        {
            Id = p.Id,
            HotelId = p.HotelId,
            RecordType = p.RecordType,
            RetentionDays = p.RetentionDays,
            IsLegalHold = p.IsLegalHold,
            Notes = p.Notes
        }).ToList();
    }
}
