using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class DeletionLog : TenantEntity
{
    public RetentionRecordType RecordType { get; set; }
    public Guid RecordId { get; set; }
    public string RecordReference { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool WasAnonymized { get; set; } = false;
}
