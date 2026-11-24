using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.CheckIn;

public record ApproveCheckInCommand(Guid SessionId) : IRequest<bool>;

public class ApproveCheckInCommandHandler : IRequestHandler<ApproveCheckInCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ApproveCheckInCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ApproveCheckInCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var session = await _context.CheckInSessions
            .Include(s => s.Booking)
                .ThenInclude(b => b!.BookingRooms)
                    .ThenInclude(br => br.Room)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.HotelId == hotelId.Value, cancellationToken);

        if (session == null)
        {
            throw new EntityNotFoundException("CheckInSession", request.SessionId);
        }

        session.Status = CheckInSessionStatus.Approved;
        session.ReviewedBy = _currentUserService.UserId;
        session.ReviewedAt = DateTime.UtcNow;

        if (session.Booking != null)
        {
            session.Booking.Status = BookingStatus.CheckedIn;
            session.Booking.UpdatedBy = _currentUserService.UserId;
            session.Booking.UpdatedAt = DateTime.UtcNow;

            foreach (var br in session.Booking.BookingRooms)
            {
                if (br.Room != null)
                {
                    br.Room.Status = RoomStatus.Occupied;
                    br.Room.UpdatedBy = _currentUserService.UserId;
                    br.Room.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
