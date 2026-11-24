using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class RetentionPolicy : TenantEntity
{
    public RetentionRecordType RecordType { get; set; }
    public int RetentionDays { get; set; }
    public bool IsLegalHold { get; set; } = false;
    public string? Notes { get; set; }
    public Guid? UpdatedBy { get; set; }
}
