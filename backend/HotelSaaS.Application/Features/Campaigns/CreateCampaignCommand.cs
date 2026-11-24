using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Campaigns;

public record CreateCampaignCommand : IRequest<CampaignDto>
{
    public string Name { get; init; } = string.Empty;
    public string Occasion { get; init; } = string.Empty;
    public string MessageTemplate { get; init; } = string.Empty;
    public DateTime? ScheduledAt { get; init; }
    public bool OnlyConsentGuests { get; init; } = true;
}

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Occasion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MessageTemplate).NotEmpty();
    }
}

public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCampaignCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CampaignDto> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            Name = request.Name,
            Occasion = request.Occasion,
            MessageTemplate = request.MessageTemplate,
            Status = request.ScheduledAt.HasValue ? CampaignStatus.Scheduled : CampaignStatus.Draft,
            ScheduledAt = request.ScheduledAt,
            CreatedBy = _currentUserService.UserId,
            UpdatedBy = _currentUserService.UserId
        };

        var guestsQuery = _context.Guests
            .Where(g => g.HotelId == hotelId.Value && !g.IsDeleted && !string.IsNullOrWhiteSpace(g.Mobile));

        if (request.OnlyConsentGuests)
        {
            guestsQuery = guestsQuery.Where(g => g.MarketingConsent);
        }

        var guests = await guestsQuery.ToListAsync(cancellationToken);

        foreach (var guest in guests)
        {
            campaign.Recipients.Add(new CampaignRecipient
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId.Value,
                CampaignId = campaign.Id,
                GuestId = guest.Id,
                Mobile = guest.Mobile,
                Channel = MessageChannel.WhatsApp,
                Status = RecipientStatus.Pending
            });
        }

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        return new CampaignDto
        {
            Id = campaign.Id,
            HotelId = campaign.HotelId,
            Name = campaign.Name,
            Occasion = campaign.Occasion,
            MessageTemplate = campaign.MessageTemplate,
            Status = campaign.Status,
            ScheduledAt = campaign.ScheduledAt,
            SentAt = campaign.SentAt,
            TotalRecipients = campaign.Recipients.Count,
            SentCount = 0,
            FailedCount = 0,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }
}
