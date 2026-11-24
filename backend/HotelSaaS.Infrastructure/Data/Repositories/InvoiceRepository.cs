using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Infrastructure.Data.Repositories;

public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<string> GetNextInvoiceNumberAsync(Guid hotelId, string prefix, CancellationToken cancellationToken = default)
    {
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId, cancellationToken);
        if (hotel == null)
        {
            throw new EntityNotFoundException(nameof(Hotel), hotelId);
        }

        hotel.InvoiceSequence++;
        await _context.SaveChangesAsync(cancellationToken);

        var cleanPrefix = string.IsNullOrWhiteSpace(prefix) ? hotel.InvoicePrefix : prefix;
        return $"{cleanPrefix}-{hotel.InvoiceSequence:D6}";
    }
}
