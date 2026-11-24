using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Application.Common.Interfaces;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<Hotel?> GetByPublicTokenAsync(Guid token, CancellationToken cancellationToken = default);
    Task<Hotel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
