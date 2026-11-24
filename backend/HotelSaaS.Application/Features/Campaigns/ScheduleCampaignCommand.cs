using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Campaigns;

public record ScheduleCampaignCommand(Guid Id, DateTime ScheduledAt) : IRequest<CampaignDto>;

public class ScheduleCampaignCommandHandler : IRequestHandler<ScheduleCampaignCommand, CampaignDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ScheduleCampaignCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CampaignDto> Handle(ScheduleCampaignCommand request, CancellationToken cancellationToken)
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

        campaign.ScheduledAt = request.ScheduledAt;
        campaign.Status = CampaignStatus.Scheduled;
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
