using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Domain.Interfaces;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<Hotel?> GetByPublicTokenAsync(Guid token, CancellationToken cancellationToken = default);
    Task<Hotel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
