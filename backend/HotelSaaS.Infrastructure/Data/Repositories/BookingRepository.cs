using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Infrastructure.Data.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Booking>> GetActiveBookingsAsync(Guid hotelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.HotelId == hotelId &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn))
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Booking?> GetByReferenceAsync(Guid hotelId, string reference, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Include(b => b.Invoices)
            .Include(b => b.BookingGuests)
            .FirstOrDefaultAsync(b => b.HotelId == hotelId && b.BookingReference == reference, cancellationToken);
    }
}
