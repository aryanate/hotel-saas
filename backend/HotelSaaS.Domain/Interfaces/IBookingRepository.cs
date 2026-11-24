using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Domain.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IReadOnlyList<Booking>> GetActiveBookingsAsync(Guid hotelId, CancellationToken cancellationToken = default);
    Task<Booking?> GetByReferenceAsync(Guid hotelId, string reference, CancellationToken cancellationToken = default);
}
