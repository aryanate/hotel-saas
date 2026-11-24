using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Hotels;

public record GetMyHotelQuery : IRequest<HotelDto>;

public class GetMyHotelQueryHandler : IRequestHandler<GetMyHotelQuery, HotelDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyHotelQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<HotelDto> Handle(GetMyHotelQuery request, CancellationToken cancellationToken)
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
