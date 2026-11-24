using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record ToggleFavoriteCommand(Guid GuestId) : IRequest<bool>;

public class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleFavoriteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var guest = await _context.Guests
            .FirstOrDefaultAsync(g => g.Id == request.GuestId && g.HotelId == hotelId.Value && !g.IsDeleted, cancellationToken);

        if (guest == null)
        {
            throw new EntityNotFoundException("Guest", request.GuestId);
        }

        guest.IsFavorite = !guest.IsFavorite;
        guest.UpdatedBy = _currentUserService.UserId;
        guest.UpdatedAt = DateTime.UtcNow;

        var fav = await _context.FavoriteGuests
            .FirstOrDefaultAsync(f => f.GuestId == guest.Id && f.HotelId == hotelId.Value, cancellationToken);

        if (guest.IsFavorite)
        {
            if (fav == null)
            {
                _context.FavoriteGuests.Add(new FavoriteGuest
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId.Value,
                    GuestId = guest.Id,
                    MarkedBy = _currentUserService.UserId,
                    MarkedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            if (fav != null)
            {
                _context.FavoriteGuests.Remove(fav);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return guest.IsFavorite;
    }
}
