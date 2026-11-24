using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.CheckIn;

public record ApproveCheckInCommand(Guid SessionId, Guid? RoomId = null) : IRequest<bool>;

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
                .ThenInclude(b => b!.Guest)
                    .ThenInclude(g => g!.Documents)
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

            if (session.Booking.Guest?.Documents != null)
            {
                foreach (var doc in session.Booking.Guest.Documents)
                {
                    doc.IsVerified = true;
                    doc.VerifiedAt = DateTime.UtcNow;
                    doc.VerifiedBy = _currentUserService.UserId;
                }
            }

            if (request.RoomId.HasValue && !session.Booking.BookingRooms.Any(br => br.RoomId == request.RoomId.Value))
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.Id == request.RoomId.Value && r.HotelId == hotelId.Value && !r.IsDeleted, cancellationToken);

                if (room != null)
                {
                    _context.BookingRooms.Add(new Domain.Entities.BookingRoom
                    {
                        Id = Guid.NewGuid(),
                        HotelId = hotelId.Value,
                        BookingId = session.Booking.Id,
                        RoomId = room.Id,
                        PricePerNight = room.PricePerNight,
                        CheckInDate = session.Booking.CheckInDate,
                        CheckOutDate = session.Booking.CheckOutDate
                    });
                    room.Status = RoomStatus.Occupied;
                    room.UpdatedBy = _currentUserService.UserId;
                    room.UpdatedAt = DateTime.UtcNow;
                }
            }

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
