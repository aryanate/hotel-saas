using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Application.Common.Models;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record GetGuestsQuery : IRequest<PagedResult<GuestDto>>
{
    public string? Search { get; init; }
    public bool? IsFavorite { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetGuestsQueryHandler : IRequestHandler<GetGuestsQuery, PagedResult<GuestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGuestsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<GuestDto>> Handle(GetGuestsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var query = _context.Guests
            .Where(g => g.HotelId == hotelId.Value && !g.IsDeleted);

        if (request.IsFavorite.HasValue)
        {
            query = query.Where(g => g.IsFavorite == request.IsFavorite.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(g => g.FullName.Contains(request.Search) ||
                                     g.Mobile.Contains(request.Search) ||
                                     (g.Email != null && g.Email.Contains(request.Search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var guests = await query
            .OrderByDescending(g => g.UpdatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = guests.Select(g => new GuestDto
        {
            Id = g.Id,
            HotelId = g.HotelId,
            FullName = g.FullName,
            Mobile = g.Mobile,
            Email = g.Email,
            Address = g.Address,
            TotalVisits = g.TotalVisits,
            TotalSpending = g.TotalSpending,
            LastVisitAt = g.LastVisitAt,
            Preferences = g.Preferences,
            IsFavorite = g.IsFavorite,
            MarketingConsent = g.MarketingConsent,
            MarketingConsentAt = g.MarketingConsentAt,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt
        }).ToList();

        return new PagedResult<GuestDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
