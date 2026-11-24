using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.CheckIn;

public record GetPublicCheckInQuery(string Token) : IRequest<PublicCheckInInfoDto>;
public record GetCheckInPageQuery(string Token) : IRequest<PublicCheckInInfoDto>;
public record GetCheckInStatusQuery(string Token) : IRequest<CheckInSessionDto>;

public class GetPublicCheckInQueryHandler :
    IRequestHandler<GetPublicCheckInQuery, PublicCheckInInfoDto>,
    IRequestHandler<GetCheckInPageQuery, PublicCheckInInfoDto>,
    IRequestHandler<GetCheckInStatusQuery, CheckInSessionDto>
{
    private readonly IApplicationDbContext _context;

    public GetPublicCheckInQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PublicCheckInInfoDto> Handle(GetPublicCheckInQuery request, CancellationToken cancellationToken)
    {
        return await GetPublicInfo(request.Token, cancellationToken);
    }

    public async Task<PublicCheckInInfoDto> Handle(GetCheckInPageQuery request, CancellationToken cancellationToken)
    {
        return await GetPublicInfo(request.Token, cancellationToken);
    }

    public async Task<CheckInSessionDto> Handle(GetCheckInStatusQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.CheckInSessions
            .Include(s => s.Booking)
                .ThenInclude(b => b!.Guest)
            .Include(s => s.Booking)
                .ThenInclude(b => b!.Hotel)
            .Include(s => s.Booking)
                .ThenInclude(b => b!.BookingRooms)
                    .ThenInclude(br => br.Room)
            .FirstOrDefaultAsync(s => s.Token == request.Token, cancellationToken);

        if (session == null)
        {
            throw new EntityNotFoundException("CheckInSession", request.Token);
        }

        return new CheckInSessionDto
        {
            Id = session.Id,
            HotelId = session.HotelId,
            BookingId = session.BookingId,
            Token = session.Token,
            TokenExpiry = session.TokenExpiry,
            Status = session.Status,
            SubmittedAt = session.SubmittedAt,
            ReviewedBy = session.ReviewedBy,
            ReviewedAt = session.ReviewedAt,
            GuestName = session.Booking?.Guest?.FullName,
            BookingReference = session.Booking?.BookingReference,
            HotelName = session.Booking?.Hotel?.Name,
            CheckInDate = session.Booking?.CheckInDate ?? DateTime.MinValue,
            CheckOutDate = session.Booking?.CheckOutDate ?? DateTime.MinValue,
            RoomNumbers = session.Booking?.BookingRooms.Select(br => br.Room?.RoomNumber ?? "").ToList() ?? new()
        };
    }

    private async Task<PublicCheckInInfoDto> GetPublicInfo(string token, CancellationToken cancellationToken)
    {
        // 1. Try finding by CheckInSession Token
        var session = await _context.CheckInSessions
            .Include(s => s.Booking)
                .ThenInclude(b => b!.Guest)
            .Include(s => s.Booking)
                .ThenInclude(b => b!.Hotel)
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);

        if (session != null)
        {
            if (session.Status == CheckInSessionStatus.Sent)
            {
                session.Status = CheckInSessionStatus.Opened;
                await _context.SaveChangesAsync(cancellationToken);
            }

            var isExpired = session.TokenExpiry < DateTime.UtcNow;

            return new PublicCheckInInfoDto
            {
                HotelName = session.Booking?.Hotel?.Name ?? "",
                HotelLogoUrl = session.Booking?.Hotel?.LogoUrl,
                GuestName = session.Booking?.Guest?.FullName ?? "",
                GuestMobile = session.Booking?.Guest?.Mobile ?? "",
                BookingReference = session.Booking?.BookingReference ?? "",
                CheckInDate = session.Booking?.CheckInDate ?? DateTime.MinValue,
                CheckOutDate = session.Booking?.CheckOutDate ?? DateTime.MinValue,
                Status = session.Status,
                IsExpired = isExpired
            };
        }

        // 2. Try finding by Hotel PublicToken (Front-desk standee QR walk-in)
        var hasGuid = Guid.TryParse(token, out var parsedGuid);
        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => !h.IsDeleted && (h.PublicToken.ToString() == token || (hasGuid && h.PublicToken == parsedGuid)), cancellationToken);

        if (hotel != null)
        {
            return new PublicCheckInInfoDto
            {
                HotelName = hotel.Name,
                HotelLogoUrl = hotel.LogoUrl,
                GuestName = "",
                GuestMobile = "",
                BookingReference = "Front-Desk Walk-In",
                CheckInDate = DateTime.UtcNow.Date,
                CheckOutDate = DateTime.UtcNow.Date.AddDays(1),
                Status = CheckInSessionStatus.Opened,
                IsExpired = false
            };
        }

        throw new EntityNotFoundException("CheckInSession", token);
    }
}
