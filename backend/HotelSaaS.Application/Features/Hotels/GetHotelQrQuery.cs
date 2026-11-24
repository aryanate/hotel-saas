using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HotelSaaS.Application.Features.Hotels;

public record GetHotelQrQuery(string? HostUrl = null) : IRequest<byte[]>;

public class GetHotelQrQueryHandler : IRequestHandler<GetHotelQrQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IConfiguration _configuration;

    public GetHotelQrQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IQrCodeService qrCodeService,
        IConfiguration configuration)
    {
        _context = context;
        _currentUserService = currentUserService;
        _qrCodeService = qrCodeService;
        _configuration = configuration;
    }

    public async Task<byte[]> Handle(GetHotelQrQuery request, CancellationToken cancellationToken)
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

        var frontendUrl = !string.IsNullOrWhiteSpace(request.HostUrl)
            ? request.HostUrl
            : (_configuration["Frontend:BaseUrl"] ?? "http://localhost:5173");
        var publicCheckInUrl = $"{frontendUrl.TrimEnd('/')}/checkin/{hotel.PublicToken}";
        return _qrCodeService.GenerateQrCodePng(publicCheckInUrl);
    }
}
