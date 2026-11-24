using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Domain.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<string> GetNextInvoiceNumberAsync(Guid hotelId, string prefix, CancellationToken cancellationToken = default);
}
