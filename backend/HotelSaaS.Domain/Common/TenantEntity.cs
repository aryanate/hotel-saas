namespace HotelSaaS.Domain.Common;

public abstract class TenantEntity : BaseEntity
{
    public Guid HotelId { get; set; }
}
