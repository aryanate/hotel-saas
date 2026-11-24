using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Bookings;

public record UpdateBookingCommand : IRequest<BookingDto>
{
    public Guid Id { get; init; }
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public int NumberOfAdults { get; init; }
    public int NumberOfChildren { get; init; }
    public string? SpecialRequests { get; init; }
    public List<Guid>? RoomIds { get; init; }
}

public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, BookingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBookingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BookingDto> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var booking = await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                    .ThenInclude(r => r!.RoomType)
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.HotelId == hotelId.Value && !b.IsDeleted, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException("Booking", request.Id);
        }

        booking.CheckInDate = request.CheckInDate;
        booking.CheckOutDate = request.CheckOutDate;
        booking.NumberOfAdults = request.NumberOfAdults;
        booking.NumberOfChildren = request.NumberOfChildren;
        booking.SpecialRequests = request.SpecialRequests;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new BookingDto
        {
            Id = booking.Id,
            HotelId = booking.HotelId,
            BookingReference = booking.BookingReference,
            GuestId = booking.GuestId,
            GuestName = booking.Guest?.FullName ?? string.Empty,
            GuestMobile = booking.Guest?.Mobile ?? string.Empty,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            Status = booking.Status,
            Source = booking.Source,
            NumberOfAdults = booking.NumberOfAdults,
            NumberOfChildren = booking.NumberOfChildren,
            TotalAmount = booking.BookingRooms.Sum(br => br.PricePerNight * Math.Max(1, (br.CheckOutDate - br.CheckInDate).Days)),
            SpecialRequests = booking.SpecialRequests,
            Rooms = booking.BookingRooms.Select(br => new BookingRoomDto
            {
                RoomId = br.RoomId,
                RoomNumber = br.Room?.RoomNumber ?? string.Empty,
                RoomTypeName = br.Room?.RoomType?.Name ?? string.Empty,
                PricePerNight = br.PricePerNight
            }).ToList(),
            CreatedAt = booking.CreatedAt
        };
    }
}

public record GetCheckInLinkQuery(Guid BookingId) : IRequest<string>;

public class GetCheckInLinkQueryHandler : IRequestHandler<GetCheckInLinkQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCheckInLinkQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(GetCheckInLinkQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var session = await _context.CheckInSessions
            .FirstOrDefaultAsync(s => s.BookingId == request.BookingId && s.HotelId == hotelId.Value, cancellationToken);

        if (session == null)
        {
            session = new CheckInSession
            {
                HotelId = hotelId.Value,
                BookingId = request.BookingId,
                Token = Guid.NewGuid().ToString("N"),
                TokenExpiry = DateTime.UtcNow.AddDays(2),
                Status = CheckInSessionStatus.Sent
            };
            _context.CheckInSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return $"/checkin/{session.Token}";
    }
}

public record SendCheckInLinkCommand(Guid BookingId) : IRequest<bool>;

public class SendCheckInLinkCommandHandler : IRequestHandler<SendCheckInLinkCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWhatsAppService _whatsAppService;

    public SendCheckInLinkCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IWhatsAppService whatsAppService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _whatsAppService = whatsAppService;
    }

    public async Task<bool> Handle(SendCheckInLinkCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var booking = await _context.Bookings
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.HotelId == hotelId.Value && !b.IsDeleted, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException("Booking", request.BookingId);
        }

        var session = await _context.CheckInSessions
            .FirstOrDefaultAsync(s => s.BookingId == request.BookingId && s.HotelId == hotelId.Value, cancellationToken);

        if (session == null)
        {
            session = new CheckInSession
            {
                HotelId = hotelId.Value,
                BookingId = request.BookingId,
                Token = Guid.NewGuid().ToString("N"),
                TokenExpiry = DateTime.UtcNow.AddDays(2),
                Status = CheckInSessionStatus.Sent
            };
            _context.CheckInSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (!string.IsNullOrEmpty(booking.Guest?.Mobile))
        {
            await _whatsAppService.SendMessageAsync(
                booking.Guest.Mobile,
                $"Welcome! Please complete your check-in: /checkin/{session.Token}",
                cancellationToken);
        }

        return true;
    }
}
