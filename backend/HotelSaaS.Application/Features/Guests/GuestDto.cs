using HotelSaaS.Domain.Enums;

namespace HotelSaaS.Application.Features.Guests;

public class GuestDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int TotalVisits { get; set; }
    public decimal TotalSpending { get; set; }
    public DateTime? LastVisitAt { get; set; }
    public string? Preferences { get; set; }
    public bool IsFavorite { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTime? MarketingConsentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GuestDetailDto : GuestDto
{
    public List<GuestNoteDto> Notes { get; set; } = new();
    public List<GuestDocumentDto> Documents { get; set; } = new();
}

public class GuestNoteDto
{
    public Guid Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GuestDocumentDto
{
    public Guid Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? StorageUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
