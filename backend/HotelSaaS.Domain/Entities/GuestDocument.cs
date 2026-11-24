using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Domain.Entities;

public class GuestDocument : TenantEntity
{
    public Guid GuestId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool IsVerified { get; set; } = false;
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? RetainUntil { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public Guest? Guest { get; set; }
}
