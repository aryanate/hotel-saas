using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Application.Common.Interfaces;

public interface IGuestRepository : IRepository<Guest>
{
    Task<Guest?> GetByMobileAsync(Guid hotelId, string mobile, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Guest> Items, int TotalCount)> GetFavoritesAsync(Guid hotelId, int page, int pageSize, CancellationToken cancellationToken = default);
}
