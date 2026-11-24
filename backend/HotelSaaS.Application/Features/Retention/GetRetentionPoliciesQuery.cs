using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Retention;

public record GetRetentionPoliciesQuery : IRequest<IReadOnlyList<RetentionPolicyDto>>;

public class GetRetentionPoliciesQueryHandler : IRequestHandler<GetRetentionPoliciesQuery, IReadOnlyList<RetentionPolicyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetRetentionPoliciesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RetentionPolicyDto>> Handle(GetRetentionPoliciesQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var policies = await _context.RetentionPolicies
            .Where(p => p.HotelId == hotelId.Value)
            .ToListAsync(cancellationToken);

        // If no policies found, generate defaults for each RetentionRecordType
        if (!policies.Any())
        {
            var defaults = Enum.GetValues<RetentionRecordType>()
                .Select(type => new RetentionPolicy
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId.Value,
                    RecordType = type,
                    RetentionDays = type == RetentionRecordType.GuestDocument ? 365 : 730,
                    IsLegalHold = false
                })
                .ToList();

            _context.RetentionPolicies.AddRange(defaults);
            await _context.SaveChangesAsync(cancellationToken);
            policies = defaults;
        }

        return policies.Select(p => new RetentionPolicyDto
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
