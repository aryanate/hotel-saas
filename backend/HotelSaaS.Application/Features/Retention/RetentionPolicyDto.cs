using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Application.Features.Retention;

public class RetentionPolicyDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public RetentionRecordType RecordType { get; set; }
    public int RetentionDays { get; set; }
    public bool IsLegalHold { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRetentionPolicyRequest
{
    public RetentionRecordType RecordType { get; set; }
    public int RetentionDays { get; set; }
    public bool IsLegalHold { get; set; }
    public string? Notes { get; set; }
}

public class DeletionPreviewDto
{
    public RetentionRecordType RecordType { get; set; }
    public int RetentionDays { get; set; }
    public int EligibleCount { get; set; }
    public DateTime CutoffDate { get; set; }
    public bool IsLegalHold { get; set; }
}
