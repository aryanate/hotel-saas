using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record GetGuestByIdQuery(Guid Id) : IRequest<GuestDetailDto>;

public class GetGuestByIdQueryHandler : IRequestHandler<GetGuestByIdQuery, GuestDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGuestByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GuestDetailDto> Handle(GetGuestByIdQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var guest = await _context.Guests
            .Include(g => g.Notes)
            .Include(g => g.Documents)
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.HotelId == hotelId.Value && !g.IsDeleted, cancellationToken);

        if (guest == null)
        {
            throw new EntityNotFoundException("Guest", request.Id);
        }

        return new GuestDetailDto
        {
            Id = guest.Id,
            HotelId = guest.HotelId,
            FullName = guest.FullName,
            Mobile = guest.Mobile,
            Email = guest.Email,
            Address = guest.Address,
            TotalVisits = guest.TotalVisits,
            TotalSpending = guest.TotalSpending,
            LastVisitAt = guest.LastVisitAt,
            Preferences = guest.Preferences,
            IsFavorite = guest.IsFavorite,
            MarketingConsent = guest.MarketingConsent,
            MarketingConsentAt = guest.MarketingConsentAt,
            CreatedAt = guest.CreatedAt,
            UpdatedAt = guest.UpdatedAt,
            Notes = guest.Notes.OrderByDescending(n => n.CreatedAt).Select(n => new GuestNoteDto
            {
                Id = n.Id,
                Note = n.Note,
                CreatedBy = n.CreatedBy,
                CreatedAt = n.CreatedAt
            }).ToList(),
            Documents = guest.Documents.Where(d => !d.IsDeleted).OrderByDescending(d => d.CreatedAt).Select(d => new GuestDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                DocumentNumber = d.DocumentNumber,
                FileName = d.FileName,
                FileSizeBytes = d.FileSizeBytes,
                IsVerified = d.IsVerified,
                VerifiedAt = d.VerifiedAt,
                CreatedAt = d.CreatedAt
            }).ToList()
        };
    }
}
