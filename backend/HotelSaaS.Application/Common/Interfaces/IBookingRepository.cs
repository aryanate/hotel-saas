using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Application.Common.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IReadOnlyList<Booking>> GetActiveBookingsAsync(Guid hotelId, CancellationToken cancellationToken = default);
    Task<Booking?> GetByReferenceAsync(Guid hotelId, string bookingReference, CancellationToken cancellationToken = default);
}
