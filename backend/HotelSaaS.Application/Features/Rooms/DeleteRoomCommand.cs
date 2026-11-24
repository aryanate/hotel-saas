using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record DeleteRoomCommand(Guid Id) : IRequest<bool>;

public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRoomCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.HotelId == hotelId.Value && !r.IsDeleted, cancellationToken);

        if (room == null)
        {
            throw new EntityNotFoundException("Room", request.Id);
        }

        var hasActiveBookings = await _context.BookingRooms
            .Include(br => br.Booking)
            .AnyAsync(br => br.RoomId == room.Id && br.Booking != null &&
                            br.Booking.Status != BookingStatus.CheckedOut &&
                            br.Booking.Status != BookingStatus.Cancelled &&
                            !br.Booking.IsDeleted, cancellationToken);

        if (hasActiveBookings)
        {
            throw new BusinessRuleViolationException("Cannot delete room with active bookings.", "ROOM_HAS_ACTIVE_BOOKINGS");
        }

        room.IsDeleted = true;
        room.UpdatedBy = _currentUserService.UserId;
        room.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
