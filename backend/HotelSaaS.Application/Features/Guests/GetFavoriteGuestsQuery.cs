using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Application.Common.Models;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record GetFavoriteGuestsQuery : IRequest<PagedResult<GuestDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetFavoriteGuestsQueryHandler : IRequestHandler<GetFavoriteGuestsQuery, PagedResult<GuestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetFavoriteGuestsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<GuestDto>> Handle(GetFavoriteGuestsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var query = _context.Guests
            .Where(g => g.HotelId == hotelId.Value && g.IsFavorite && !g.IsDeleted);

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
