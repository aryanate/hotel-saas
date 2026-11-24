using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Hotels;

public record UpdateHotelCommand : IRequest<HotelDto>
{
    public string Name { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PinCode { get; init; } = string.Empty;
    public string? GstIn { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string InvoicePrefix { get; init; } = "INV";
    public TimeOnly CheckInTime { get; init; } = new TimeOnly(14, 0);
    public TimeOnly CheckOutTime { get; init; } = new TimeOnly(11, 0);
    public string Currency { get; init; } = "INR";
    public string TimeZone { get; init; } = "Asia/Kolkata";
}

public class UpdateHotelCommandValidator : AbstractValidator<UpdateHotelCommand>
{
    public UpdateHotelCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Mobile).NotEmpty().MaximumLength(20);
        RuleFor(x => x.City).NotEmpty().MaximumLength(50);
    }
}

public class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand, HotelDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateHotelCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<HotelDto> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => h.Id == hotelId.Value && !h.IsDeleted, cancellationToken);

        if (hotel == null)
        {
            throw new EntityNotFoundException("Hotel", hotelId.Value);
        }

        hotel.Name = request.Name;
        hotel.OwnerName = request.OwnerName;
        hotel.Mobile = request.Mobile;
        hotel.Address = request.Address;
        hotel.City = request.City;
        hotel.State = request.State;
        hotel.PinCode = request.PinCode;
        hotel.GstIn = request.GstIn;
        hotel.ContactEmail = request.ContactEmail;
        hotel.ContactPhone = request.ContactPhone;
        hotel.InvoicePrefix = request.InvoicePrefix;
        hotel.CheckInTime = request.CheckInTime;
        hotel.CheckOutTime = request.CheckOutTime;
        hotel.Currency = request.Currency;
        hotel.TimeZone = request.TimeZone;
        hotel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new HotelDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            OwnerName = hotel.OwnerName,
            Email = hotel.Email,
            Mobile = hotel.Mobile,
            Address = hotel.Address,
            City = hotel.City,
            State = hotel.State,
            PinCode = hotel.PinCode,
            NumberOfRooms = hotel.NumberOfRooms,
            PublicToken = hotel.PublicToken,
            LogoUrl = hotel.LogoUrl,
            GstIn = hotel.GstIn,
            ContactEmail = hotel.ContactEmail,
            ContactPhone = hotel.ContactPhone,
            InvoicePrefix = hotel.InvoicePrefix,
            InvoiceSequence = hotel.InvoiceSequence,
            CheckInTime = hotel.CheckInTime,
            CheckOutTime = hotel.CheckOutTime,
            Currency = hotel.Currency,
            TimeZone = hotel.TimeZone,
            IsActive = hotel.IsActive
        };
    }
}
