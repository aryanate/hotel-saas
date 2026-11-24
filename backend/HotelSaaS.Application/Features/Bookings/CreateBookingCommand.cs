using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Bookings;

public record CreateBookingCommand : IRequest<BookingDto>
{
    public Guid? GuestId { get; init; }
    public string GuestName { get; init; } = string.Empty;
    public string GuestMobile { get; init; } = string.Empty;
    public string? GuestEmail { get; init; }
    public List<Guid> RoomIds { get; init; } = new();
    public BookingSource Source { get; init; } = BookingSource.Manual;
    public int NumberOfAdults { get; init; } = 1;
    public int NumberOfChildren { get; init; } = 0;
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public string? SpecialRequests { get; init; }
}

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.GuestName).NotEmpty().When(x => !x.GuestId.HasValue);
        RuleFor(x => x.GuestMobile).NotEmpty().When(x => !x.GuestId.HasValue);
        RuleFor(x => x.RoomIds).NotEmpty().WithMessage("At least one room must be selected.");
        RuleFor(x => x.CheckInDate).NotEmpty();
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after check-in date.");
        RuleFor(x => x.NumberOfAdults).GreaterThan(0);
    }
}

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateBookingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        // Check room conflicts
        var conflictingRooms = await _context.BookingRooms
            .Include(br => br.Booking)
            .Include(br => br.Room)
            .Where(br => br.HotelId == hotelId.Value &&
                         request.RoomIds.Contains(br.RoomId) &&
                         br.Booking != null &&
                         br.Booking.Status != BookingStatus.Cancelled &&
                         br.Booking.Status != BookingStatus.CheckedOut &&
                         !br.Booking.IsDeleted &&
                         request.CheckInDate < br.CheckOutDate && request.CheckOutDate > br.CheckInDate)
            .Select(br => br.Room != null ? br.Room.RoomNumber : br.RoomId.ToString())
            .ToListAsync(cancellationToken);

        if (conflictingRooms.Any())
        {
            throw new BusinessRuleViolationException(
                $"Rooms already booked for the selected dates: {string.Join(", ", conflictingRooms)}",
                "ROOM_NOT_AVAILABLE");
        }

        // Find or create guest
        Guest? guest = null;
        if (request.GuestId.HasValue)
        {
            guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Id == request.GuestId.Value && g.HotelId == hotelId.Value && !g.IsDeleted, cancellationToken);
            if (guest == null)
            {
                throw new EntityNotFoundException("Guest", request.GuestId.Value);
            }
        }
        else
        {
            guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.HotelId == hotelId.Value && g.Mobile == request.GuestMobile && !g.IsDeleted, cancellationToken);

            if (guest == null)
            {
                guest = new Guest
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId.Value,
                    FullName = request.GuestName,
                    Mobile = request.GuestMobile,
                    Email = request.GuestEmail,
                    CreatedBy = _currentUserService.UserId,
                    UpdatedBy = _currentUserService.UserId
                };
                _context.Guests.Add(guest);
            }
        }

        // Fetch selected rooms
        var rooms = await _context.Rooms
            .Include(r => r.RoomType)
            .Where(r => r.HotelId == hotelId.Value && request.RoomIds.Contains(r.Id) && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        if (rooms.Count != request.RoomIds.Count)
        {
            throw new BusinessRuleViolationException("One or more selected rooms were not found.", "ROOM_NOT_FOUND");
        }

        var refCode = $"BK-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            GuestId = guest.Id,
            BookingReference = refCode,
            Source = request.Source,
            Status = BookingStatus.Confirmed,
            NumberOfAdults = request.NumberOfAdults,
            NumberOfChildren = request.NumberOfChildren,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            SpecialRequests = request.SpecialRequests,
            CreatedBy = _currentUserService.UserId,
            UpdatedBy = _currentUserService.UserId
        };

        foreach (var room in rooms)
        {
            booking.BookingRooms.Add(new BookingRoom
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId.Value,
                BookingId = booking.Id,
                RoomId = room.Id,
                PricePerNight = room.PricePerNight,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate
            });

            if (request.CheckInDate.Date == DateTime.UtcNow.Date)
            {
                room.Status = RoomStatus.Reserved;
            }
        }

        booking.BookingGuests.Add(new BookingGuest
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            BookingId = booking.Id,
            GuestId = guest.Id,
            IsPrimary = true
        });

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        return new BookingDto
        {
            Id = booking.Id,
            HotelId = booking.HotelId,
            GuestId = guest.Id,
            GuestName = guest.FullName,
            GuestMobile = guest.Mobile,
            GuestEmail = guest.Email,
            BookingReference = booking.BookingReference,
            Source = booking.Source,
            Status = booking.Status,
            NumberOfAdults = booking.NumberOfAdults,
            NumberOfChildren = booking.NumberOfChildren,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            TotalNights = booking.TotalNights,
            SpecialRequests = booking.SpecialRequests,
            TotalAmount = booking.BookingRooms.Sum(br => br.PricePerNight * Math.Max(1, (br.CheckOutDate - br.CheckInDate).Days)),
            Rooms = rooms.Select(r => new BookingRoomDto
            {
                RoomId = r.Id,
                RoomNumber = r.RoomNumber,
                RoomTypeName = r.RoomType?.Name,
                PricePerNight = r.PricePerNight,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate
            }).ToList(),
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }
}
