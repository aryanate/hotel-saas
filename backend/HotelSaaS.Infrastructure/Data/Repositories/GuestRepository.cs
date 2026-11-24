using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Infrastructure.Data.Repositories;

public class GuestRepository : GenericRepository<Guest>, IGuestRepository
{
    public GuestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Guest?> GetByMobileAsync(Guid hotelId, string mobile, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(g => g.HotelId == hotelId && g.Mobile == mobile, cancellationToken);
    }

    public async Task<(IReadOnlyList<Guest> Items, int TotalCount)> GetFavoritesAsync(
        Guid hotelId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FavoriteGuests
            .Where(f => f.HotelId == hotelId)
            .Include(f => f.Guest)
            .OrderByDescending(f => f.MarkedAt)
            .Select(f => f.Guest!);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
