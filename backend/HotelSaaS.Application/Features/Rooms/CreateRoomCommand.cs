using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record CreateRoomCommand : IRequest<RoomDto>
{
    public string RoomNumber { get; init; } = string.Empty;
    public Guid RoomTypeId { get; init; }
    public int Floor { get; init; }
    public int Capacity { get; init; }
    public decimal PricePerNight { get; init; }
    public string Description { get; init; } = string.Empty;
    public RoomStatus Status { get; init; } = RoomStatus.Available;
}

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.PricePerNight).GreaterThanOrEqualTo(0);
    }
}

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateRoomCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var exists = await _context.Rooms
            .AnyAsync(r => r.HotelId == hotelId.Value && r.RoomNumber == request.RoomNumber && !r.IsDeleted, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleViolationException($"Room number '{request.RoomNumber}' already exists.", "ROOM_EXISTS");
        }

        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == request.RoomTypeId && rt.HotelId == hotelId.Value && !rt.IsDeleted, cancellationToken);

        if (roomType == null)
        {
            throw new EntityNotFoundException("RoomType", request.RoomTypeId);
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            RoomNumber = request.RoomNumber,
            RoomTypeId = request.RoomTypeId,
            Floor = request.Floor,
            Capacity = request.Capacity,
            PricePerNight = request.PricePerNight,
            Description = request.Description,
            Status = request.Status,
            CreatedBy = _currentUserService.UserId,
            UpdatedBy = _currentUserService.UserId
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoomDto
        {
            Id = room.Id,
            HotelId = room.HotelId,
            RoomNumber = room.RoomNumber,
            RoomTypeId = room.RoomTypeId,
            RoomTypeName = roomType.Name,
            Floor = room.Floor,
            Capacity = room.Capacity,
            PricePerNight = room.PricePerNight,
            Description = room.Description,
            Status = room.Status,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };
    }
}
