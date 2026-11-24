using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.CheckIn;

public record GetPendingCheckInsQuery : IRequest<IReadOnlyList<CheckInSessionDto>>;

public class GetPendingCheckInsQueryHandler : IRequestHandler<GetPendingCheckInsQuery, IReadOnlyList<CheckInSessionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPendingCheckInsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CheckInSessionDto>> Handle(GetPendingCheckInsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var sessions = await _context.CheckInSessions
            .Include(s => s.Booking)
                .ThenInclude(b => b!.Guest)
                    .ThenInclude(g => g!.Documents)
            .Include(s => s.Booking)
                .ThenInclude(b => b!.BookingRooms)
                    .ThenInclude(br => br.Room)
            .Where(s => s.HotelId == hotelId.Value && s.Status == CheckInSessionStatus.Submitted)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

        return sessions.Select(s =>
        {
            var guest = s.Booking?.Guest;
            var docs = guest?.Documents?.Where(d => !d.IsDeleted).ToList() ?? new();
            var doc = docs.FirstOrDefault();
            var docUrls = docs
                .Where(d => !string.IsNullOrEmpty(d.StorageKey) && d.StorageKey != "manual-entry" && d.StorageKey.StartsWith("uploads"))
                .Select(d => d.StorageKey.StartsWith("/") ? d.StorageKey : $"/{d.StorageKey}")
                .ToList();

            return new CheckInSessionDto
            {
                Id = s.Id,
                HotelId = s.HotelId,
                BookingId = s.BookingId,
                Token = s.Token,
                TokenExpiry = s.TokenExpiry,
                Status = s.Status,
                SubmittedAt = s.SubmittedAt,
                ReviewedBy = s.ReviewedBy,
                ReviewedAt = s.ReviewedAt,
                GuestName = guest?.FullName,
                GuestMobile = guest?.Mobile,
                GuestEmail = guest?.Email,
                GuestAddress = guest?.Address,
                DocumentType = doc?.DocumentType.ToString(),
                DocumentNumber = doc?.DocumentNumber,
                DocumentUrl = docUrls.FirstOrDefault(),
                DocumentUrls = docUrls,
                BookingReference = s.Booking?.BookingReference,
                CheckInDate = s.Booking?.CheckInDate ?? DateTime.MinValue,
                CheckOutDate = s.Booking?.CheckOutDate ?? DateTime.MinValue,
                RoomNumbers = s.Booking?.BookingRooms.Select(br => br.Room?.RoomNumber ?? string.Empty).ToList() ?? new()
            };
        }).ToList();
    }
}
