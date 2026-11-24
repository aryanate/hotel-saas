using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Domain.Interfaces;

public interface IGuestRepository : IRepository<Guest>
{
    Task<Guest?> GetByMobileAsync(Guid hotelId, string mobile, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Guest> Items, int TotalCount)> GetFavoritesAsync(Guid hotelId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}
