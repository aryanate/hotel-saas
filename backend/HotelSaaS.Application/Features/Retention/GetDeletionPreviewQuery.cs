using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Retention;

public record GetDeletionPreviewQuery : IRequest<IReadOnlyList<DeletionPreviewDto>>;

public class GetDeletionPreviewQueryHandler : IRequestHandler<GetDeletionPreviewQuery, IReadOnlyList<DeletionPreviewDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDeletionPreviewQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<DeletionPreviewDto>> Handle(GetDeletionPreviewQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var policies = await _context.RetentionPolicies
            .Where(p => p.HotelId == hotelId.Value)
            .ToListAsync(cancellationToken);

        var preview = new List<DeletionPreviewDto>();

        foreach (var policy in policies)
        {
            var cutoff = DateTime.UtcNow.AddDays(-policy.RetentionDays);
            int eligibleCount = 0;

            if (!policy.IsLegalHold)
            {
                switch (policy.RecordType)
                {
                    case RetentionRecordType.GuestDocument:
                        eligibleCount = await _context.GuestDocuments
                            .CountAsync(d => d.HotelId == hotelId.Value && !d.IsDeleted && d.CreatedAt < cutoff, cancellationToken);
                        break;
                    case RetentionRecordType.BookingRecord:
                        eligibleCount = await _context.Bookings
                            .CountAsync(b => b.HotelId == hotelId.Value && !b.IsDeleted && b.CheckOutDate < cutoff, cancellationToken);
                        break;
                    case RetentionRecordType.InvoiceRecord:
                        eligibleCount = await _context.Invoices
                            .CountAsync(i => i.HotelId == hotelId.Value && !i.IsDeleted && i.InvoiceDate < cutoff, cancellationToken);
                        break;
                    case RetentionRecordType.AuditRecord:
                        eligibleCount = await _context.AuditLogs
                            .CountAsync(a => a.HotelId == hotelId.Value && a.CreatedAt < cutoff, cancellationToken);
                        break;
                    default:
                        break;
                }
            }

            preview.Add(new DeletionPreviewDto
            {
                RecordType = policy.RecordType,
                RetentionDays = policy.RetentionDays,
                EligibleCount = eligibleCount,
                CutoffDate = cutoff,
                IsLegalHold = policy.IsLegalHold
            });
        }

        return preview;
    }
}
