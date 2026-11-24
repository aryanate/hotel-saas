using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.CheckIn;

public record SubmitPublicCheckInCommand : IRequest<bool>
{
    public string Token { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? Mobile { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public DocumentType DocumentType { get; init; } = DocumentType.Other;
    public string? DocumentNumber { get; init; }
    public Stream? DocumentStream { get; init; }
    public string? DocumentFileName { get; init; }
    public bool MarketingConsent { get; init; }
}

public record SubmitCheckInCommand : SubmitPublicCheckInCommand;

public class SubmitPublicCheckInCommandValidator : AbstractValidator<SubmitPublicCheckInCommand>
{
    public SubmitPublicCheckInCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
    }
}

public class SubmitPublicCheckInCommandHandler :
    IRequestHandler<SubmitPublicCheckInCommand, bool>,
    IRequestHandler<SubmitCheckInCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public SubmitPublicCheckInCommandHandler(
        IApplicationDbContext context,
        IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<bool> Handle(SubmitPublicCheckInCommand request, CancellationToken cancellationToken)
    {
        return await ProcessSubmission(request, cancellationToken);
    }

    public async Task<bool> Handle(SubmitCheckInCommand request, CancellationToken cancellationToken)
    {
        return await ProcessSubmission(request, cancellationToken);
    }

    private async Task<bool> ProcessSubmission(SubmitPublicCheckInCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.CheckInSessions
            .Include(s => s.Booking)
                .ThenInclude(b => b!.Guest)
            .FirstOrDefaultAsync(s => s.Token == request.Token, cancellationToken);

        if (session == null)
        {
            // Check if this is a walk-in check-in via Hotel.PublicToken
            var hasGuid = Guid.TryParse(request.Token, out var parsedGuid);
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => !h.IsDeleted && (h.PublicToken.ToString() == request.Token || (hasGuid && h.PublicToken == parsedGuid)), cancellationToken);

            if (hotel == null)
            {
                throw new EntityNotFoundException("CheckInSession", request.Token);
            }

            // Create walk-in Guest
            var walkInGuest = new Guest
            {
                Id = Guid.NewGuid(),
                HotelId = hotel.Id,
                FullName = request.FullName,
                Mobile = !string.IsNullOrWhiteSpace(request.Mobile) ? request.Mobile : "0000000000",
                Email = request.Email,
                Address = request.Address,
                TotalVisits = 1,
                TotalSpending = 0m,
                LastVisitAt = DateTime.UtcNow,
                MarketingConsent = request.MarketingConsent,
                MarketingConsentAt = request.MarketingConsent ? DateTime.UtcNow : null,
                IsDeleted = false
            };
            _context.Guests.Add(walkInGuest);

            // Create Booking
            var walkInBooking = new Booking
            {
                Id = Guid.NewGuid(),
                HotelId = hotel.Id,
                GuestId = walkInGuest.Id,
                BookingReference = $"QR-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Source = BookingSource.QR,
                Status = BookingStatus.Pending,
                NumberOfAdults = 1,
                NumberOfChildren = 0,
                CheckInDate = DateTime.UtcNow.Date,
                CheckOutDate = DateTime.UtcNow.Date.AddDays(1),
                SpecialRequests = "Front-Desk QR Walk-In Registration",
                IsDeleted = false
            };
            _context.Bookings.Add(walkInBooking);

            // Create CheckInSession in Submitted status
            session = new CheckInSession
            {
                Id = Guid.NewGuid(),
                HotelId = hotel.Id,
                BookingId = walkInBooking.Id,
                Token = Guid.NewGuid().ToString("N").Substring(0, 16),
                TokenExpiry = DateTime.UtcNow.AddDays(1),
                Status = CheckInSessionStatus.Submitted,
                SubmittedAt = DateTime.UtcNow
            };
            _context.CheckInSessions.Add(session);

            // Attach document if provided
            if (request.DocumentStream != null && !string.IsNullOrWhiteSpace(request.DocumentFileName))
            {
                var storageKey = await _storageService.UploadFileAsync(
                    request.DocumentStream,
                    request.DocumentFileName,
                    "guest-documents",
                    cancellationToken);

                var doc = new GuestDocument
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotel.Id,
                    GuestId = walkInGuest.Id,
                    DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber,
                    StorageKey = storageKey,
                    FileName = request.DocumentFileName,
                    FileSizeBytes = request.DocumentStream.Length,
                    IsVerified = false
                };
                _context.GuestDocuments.Add(doc);
            }
            else if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
            {
                var doc = new GuestDocument
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotel.Id,
                    GuestId = walkInGuest.Id,
                    DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber,
                    StorageKey = "manual-entry",
                    FileName = $"{request.DocumentType}.txt",
                    FileSizeBytes = 0,
                    IsVerified = false
                };
                _context.GuestDocuments.Add(doc);
            }

            // Record consent
            _context.GuestConsents.Add(new GuestConsent
            {
                Id = Guid.NewGuid(),
                HotelId = hotel.Id,
                GuestId = walkInGuest.Id,
                ConsentType = ConsentType.CheckIn,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        if (session.TokenExpiry < DateTime.UtcNow)
        {
            session.Status = CheckInSessionStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            throw new BusinessRuleViolationException("Check-in link has expired.", "CHECKIN_EXPIRED");
        }

        if (session.Status == CheckInSessionStatus.Submitted || session.Status == CheckInSessionStatus.Approved)
        {
            throw new BusinessRuleViolationException("Check-in has already been submitted.", "CHECKIN_ALREADY_SUBMITTED");
        }

        var guest = session.Booking?.Guest;
        if (guest != null)
        {
            guest.FullName = request.FullName;
            if (!string.IsNullOrWhiteSpace(request.Mobile)) guest.Mobile = request.Mobile;
            if (!string.IsNullOrWhiteSpace(request.Email)) guest.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.Address)) guest.Address = request.Address;
            guest.MarketingConsent = request.MarketingConsent;
            if (request.MarketingConsent) guest.MarketingConsentAt = DateTime.UtcNow;

            // Upload document if provided
            if (request.DocumentStream != null && !string.IsNullOrWhiteSpace(request.DocumentFileName))
            {
                var storageKey = await _storageService.UploadFileAsync(
                    request.DocumentStream,
                    request.DocumentFileName,
                    "guest-documents",
                    cancellationToken);

                var doc = new GuestDocument
                {
                    Id = Guid.NewGuid(),
                    HotelId = session.HotelId,
                    GuestId = guest.Id,
                    DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber,
                    StorageKey = storageKey,
                    FileName = request.DocumentFileName,
                    FileSizeBytes = request.DocumentStream.Length,
                    IsVerified = false
                };

                _context.GuestDocuments.Add(doc);
            }
            else if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
            {
                var doc = new GuestDocument
                {
                    Id = Guid.NewGuid(),
                    HotelId = session.HotelId,
                    GuestId = guest.Id,
                    DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber,
                    StorageKey = "manual-entry",
                    FileName = $"{request.DocumentType}.txt",
                    FileSizeBytes = 0,
                    IsVerified = false
                };
                _context.GuestDocuments.Add(doc);
            }

            // Record consent
            _context.GuestConsents.Add(new GuestConsent
            {
                Id = Guid.NewGuid(),
                HotelId = session.HotelId,
                GuestId = guest.Id,
                ConsentType = ConsentType.CheckIn,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow
            });
        }

        session.Status = CheckInSessionStatus.Submitted;
        session.SubmittedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
