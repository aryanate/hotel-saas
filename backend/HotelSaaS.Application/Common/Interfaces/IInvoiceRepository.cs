using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Application.Common.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<string> GetNextInvoiceNumberAsync(Guid hotelId, string prefix, CancellationToken cancellationToken = default);
}
