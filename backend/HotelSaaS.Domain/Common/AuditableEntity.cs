namespace HotelSaaS.Domain.Common;

public abstract class AuditableEntity : TenantEntity
{
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
