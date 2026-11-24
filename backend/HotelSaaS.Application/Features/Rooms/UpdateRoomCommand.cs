using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record UpdateRoomCommand : IRequest<RoomDto>
{
    public Guid Id { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public Guid RoomTypeId { get; init; }
    public int Floor { get; init; }
    public int Capacity { get; init; }
    public decimal PricePerNight { get; init; }
    public string Description { get; init; } = string.Empty;
    public RoomStatus Status { get; init; }
}

public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.PricePerNight).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, RoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRoomCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RoomDto> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var room = await _context.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.HotelId == hotelId.Value && !r.IsDeleted, cancellationToken);

        if (room == null)
        {
            throw new EntityNotFoundException("Room", request.Id);
        }

        var duplicateNumber = await _context.Rooms
            .AnyAsync(r => r.HotelId == hotelId.Value && r.RoomNumber == request.RoomNumber && r.Id != request.Id && !r.IsDeleted, cancellationToken);

        if (duplicateNumber)
        {
            throw new BusinessRuleViolationException($"Room number '{request.RoomNumber}' is already used by another room.", "ROOM_EXISTS");
        }

        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == request.RoomTypeId && rt.HotelId == hotelId.Value && !rt.IsDeleted, cancellationToken);

        if (roomType == null)
        {
            throw new EntityNotFoundException("RoomType", request.RoomTypeId);
        }

        room.RoomNumber = request.RoomNumber;
        room.RoomTypeId = request.RoomTypeId;
        room.Floor = request.Floor;
        room.Capacity = request.Capacity;
        room.PricePerNight = request.PricePerNight;
        room.Description = request.Description;
        room.Status = request.Status;
        room.UpdatedBy = _currentUserService.UserId;
        room.UpdatedAt = DateTime.UtcNow;

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
