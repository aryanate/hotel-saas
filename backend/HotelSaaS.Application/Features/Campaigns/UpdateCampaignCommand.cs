using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Campaigns;

public record UpdateCampaignCommand : IRequest<CampaignDto>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Occasion { get; init; } = string.Empty;
    public string MessageTemplate { get; init; } = string.Empty;
}

public class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Occasion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MessageTemplate).NotEmpty();
    }
}

public class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand, CampaignDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCampaignCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CampaignDto> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var campaign = await _context.Campaigns
            .Include(c => c.Recipients)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.HotelId == hotelId.Value && !c.IsDeleted, cancellationToken);

        if (campaign == null)
        {
            throw new EntityNotFoundException("Campaign", request.Id);
        }

        if (campaign.Status == CampaignStatus.Sent || campaign.Status == CampaignStatus.Sending)
        {
            throw new BusinessRuleViolationException("Cannot edit an already sent or sending campaign.", "CAMPAIGN_ALREADY_SENT");
        }

        campaign.Name = request.Name;
        campaign.Occasion = request.Occasion;
        campaign.MessageTemplate = request.MessageTemplate;
        campaign.UpdatedBy = _currentUserService.UserId;
        campaign.UpdatedAt = DateTime.UtcNow;

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
            SentCount = campaign.Recipients.Count(r => r.Status == RecipientStatus.Sent),
            FailedCount = campaign.Recipients.Count(r => r.Status == RecipientStatus.Failed),
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }
}
