using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Infrastructure.Data.Repositories;

public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
{
    public HotelRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Hotel?> GetByPublicTokenAsync(Guid token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.PublicToken == token, cancellationToken);
    }

    public async Task<Hotel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.Email.ToLower() == email.ToLower(), cancellationToken);
    }
}
