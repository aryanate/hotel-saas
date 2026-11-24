using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record GetGuestDocumentsQuery(Guid GuestId) : IRequest<IReadOnlyList<GuestDocumentDto>>;

public class GetGuestDocumentsQueryHandler : IRequestHandler<GetGuestDocumentsQuery, IReadOnlyList<GuestDocumentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetGuestDocumentsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<GuestDocumentDto>> Handle(GetGuestDocumentsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var docs = await _context.GuestDocuments
            .Where(d => d.GuestId == request.GuestId && d.HotelId == hotelId.Value && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(d => new GuestDocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            DocumentNumber = d.DocumentNumber,
            FileName = d.FileName,
            FileSizeBytes = d.FileSizeBytes,
            IsVerified = d.IsVerified,
            VerifiedAt = d.VerifiedAt,
            CreatedAt = d.CreatedAt
        }).ToList();
    }
}

public record GetDocumentUrlQuery(Guid GuestId, Guid DocumentId) : IRequest<string>;

public class GetDocumentUrlQueryHandler : IRequestHandler<GetDocumentUrlQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStorageService _storageService;

    public GetDocumentUrlQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IStorageService storageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storageService = storageService;
    }

    public async Task<string> Handle(GetDocumentUrlQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var doc = await _context.GuestDocuments
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.GuestId == request.GuestId && d.HotelId == hotelId.Value && !d.IsDeleted, cancellationToken);

        if (doc == null)
        {
            throw new EntityNotFoundException("GuestDocument", request.DocumentId);
        }

        return await _storageService.GetPreSignedUrlAsync(doc.StorageKey, 15, cancellationToken);
    }
}
