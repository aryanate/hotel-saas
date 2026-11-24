using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record UpdateGuestCommand : IRequest<GuestDto>
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Preferences { get; init; }
    public bool MarketingConsent { get; init; }
}

public class UpdateGuestCommandValidator : AbstractValidator<UpdateGuestCommand>
{
    public UpdateGuestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Mobile).NotEmpty().MaximumLength(20);
    }
}

public class UpdateGuestCommandHandler : IRequestHandler<UpdateGuestCommand, GuestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateGuestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GuestDto> Handle(UpdateGuestCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var guest = await _context.Guests
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.HotelId == hotelId.Value && !g.IsDeleted, cancellationToken);

        if (guest == null)
        {
            throw new EntityNotFoundException("Guest", request.Id);
        }

        guest.FullName = request.FullName;
        guest.Mobile = request.Mobile;
        guest.Email = request.Email;
        guest.Address = request.Address;
        guest.Preferences = request.Preferences;
        if (guest.MarketingConsent != request.MarketingConsent)
        {
            guest.MarketingConsent = request.MarketingConsent;
            guest.MarketingConsentAt = request.MarketingConsent ? DateTime.UtcNow : null;
        }
        guest.UpdatedBy = _currentUserService.UserId;
        guest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new GuestDto
        {
            Id = guest.Id,
            HotelId = guest.HotelId,
            FullName = guest.FullName,
            Mobile = guest.Mobile,
            Email = guest.Email,
            Address = guest.Address,
            TotalVisits = guest.TotalVisits,
            TotalSpending = guest.TotalSpending,
            LastVisitAt = guest.LastVisitAt,
            Preferences = guest.Preferences,
            IsFavorite = guest.IsFavorite,
            MarketingConsent = guest.MarketingConsent,
            MarketingConsentAt = guest.MarketingConsentAt,
            CreatedAt = guest.CreatedAt,
            UpdatedAt = guest.UpdatedAt
        };
    }
}
