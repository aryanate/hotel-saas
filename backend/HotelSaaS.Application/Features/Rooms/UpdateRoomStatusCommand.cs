using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record UpdateRoomStatusCommand : IRequest<RoomDto>
{
    public Guid Id { get; init; }
    public RoomStatus Status { get; init; }
}

public class UpdateRoomStatusCommandValidator : AbstractValidator<UpdateRoomStatusCommand>
{
    public UpdateRoomStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class UpdateRoomStatusCommandHandler : IRequestHandler<UpdateRoomStatusCommand, RoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRoomStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RoomDto> Handle(UpdateRoomStatusCommand request, CancellationToken cancellationToken)
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
            RoomTypeName = room.RoomType?.Name,
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
