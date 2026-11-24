using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Rooms;

public record CreateRoomTypeCommand : IRequest<RoomTypeDto>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public int MaxCapacity { get; init; }
}

public class CreateRoomTypeCommandValidator : AbstractValidator<CreateRoomTypeCommand>
{
    public CreateRoomTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCapacity).GreaterThan(0);
    }
}

public class CreateRoomTypeCommandHandler : IRequestHandler<CreateRoomTypeCommand, RoomTypeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateRoomTypeCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<RoomTypeDto> Handle(CreateRoomTypeCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var exists = await _context.RoomTypes
            .AnyAsync(rt => rt.HotelId == hotelId.Value && rt.Name == request.Name && !rt.IsDeleted, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleViolationException($"Room type '{request.Name}' already exists.", "ROOM_TYPE_EXISTS");
        }

        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice,
            MaxCapacity = request.MaxCapacity,
            CreatedBy = _currentUserService.UserId,
            UpdatedBy = _currentUserService.UserId
        };

        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoomTypeDto
        {
            Id = roomType.Id,
            HotelId = roomType.HotelId,
            Name = roomType.Name,
            Description = roomType.Description,
            BasePrice = roomType.BasePrice,
            MaxCapacity = roomType.MaxCapacity,
            TotalRooms = 0
        };
    }
}
