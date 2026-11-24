using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Campaigns;

public record SendCampaignCommand(Guid Id) : IRequest<CampaignDto>;
public record SendCampaignNowCommand(Guid Id) : IRequest<CampaignDto>;

public class SendCampaignCommandHandler :
    IRequestHandler<SendCampaignCommand, CampaignDto>,
    IRequestHandler<SendCampaignNowCommand, CampaignDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWhatsAppService _whatsAppService;

    public SendCampaignCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IWhatsAppService whatsAppService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _whatsAppService = whatsAppService;
    }

    public async Task<CampaignDto> Handle(SendCampaignCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteSend(request.Id, cancellationToken);
    }

    public async Task<CampaignDto> Handle(SendCampaignNowCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteSend(request.Id, cancellationToken);
    }

    private async Task<CampaignDto> ExecuteSend(Guid campaignId, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var campaign = await _context.Campaigns
            .Include(c => c.Recipients)
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.HotelId == hotelId.Value && !c.IsDeleted, cancellationToken);

        if (campaign == null)
        {
            throw new EntityNotFoundException("Campaign", campaignId);
        }

        campaign.Status = CampaignStatus.Sending;
        await _context.SaveChangesAsync(cancellationToken);

        int sent = 0;
        int failed = 0;

        foreach (var recipient in campaign.Recipients.Where(r => r.Status == RecipientStatus.Pending))
        {
            try
            {
                var externalId = await _whatsAppService.SendMessageAsync(
                    recipient.Mobile,
                    campaign.MessageTemplate,
                    cancellationToken);

                recipient.Status = RecipientStatus.Sent;
                recipient.SentAt = DateTime.UtcNow;
                recipient.ExternalMessageId = externalId;
                sent++;
            }
            catch (Exception ex)
            {
                recipient.Status = RecipientStatus.Failed;
                recipient.ErrorReason = ex.Message;
                failed++;
            }
        }

        campaign.Status = failed > 0 && sent == 0 ? CampaignStatus.Failed : CampaignStatus.Sent;
        campaign.SentAt = DateTime.UtcNow;
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
