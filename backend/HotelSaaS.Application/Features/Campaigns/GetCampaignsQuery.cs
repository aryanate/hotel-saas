using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Campaigns;

public record GetCampaignsQuery : IRequest<IReadOnlyList<CampaignDto>>
{
    public CampaignStatus? Status { get; init; }
}

public class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, IReadOnlyList<CampaignDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCampaignsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CampaignDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var query = _context.Campaigns
            .Include(c => c.Recipients)
            .Where(c => c.HotelId == hotelId.Value && !c.IsDeleted);

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return campaigns.Select(c => new CampaignDto
        {
            Id = c.Id,
            HotelId = c.HotelId,
            Name = c.Name,
            Occasion = c.Occasion,
            MessageTemplate = c.MessageTemplate,
            Status = c.Status,
            ScheduledAt = c.ScheduledAt,
            SentAt = c.SentAt,
            TotalRecipients = c.Recipients.Count,
            SentCount = c.Recipients.Count(r => r.Status == RecipientStatus.Sent),
            FailedCount = c.Recipients.Count(r => r.Status == RecipientStatus.Failed),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}

public record GetCampaignRecipientsQuery(Guid CampaignId) : IRequest<IReadOnlyList<CampaignRecipientDto>>;

public class GetCampaignRecipientsQueryHandler : IRequestHandler<GetCampaignRecipientsQuery, IReadOnlyList<CampaignRecipientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCampaignRecipientsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CampaignRecipientDto>> Handle(GetCampaignRecipientsQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var recipients = await _context.CampaignRecipients
            .Include(r => r.Guest)
            .Where(r => r.CampaignId == request.CampaignId && r.HotelId == hotelId.Value)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return recipients.Select(r => new CampaignRecipientDto
        {
            Id = r.Id,
            CampaignId = r.CampaignId,
            GuestId = r.GuestId,
            GuestName = r.Guest?.FullName,
            Mobile = r.Mobile,
            Channel = r.Channel,
            Status = r.Status,
            SentAt = r.SentAt,
            ExternalMessageId = r.ExternalMessageId,
            ErrorReason = r.ErrorReason
        }).ToList();
    }
}

public record DeleteCampaignCommand(Guid Id) : IRequest<bool>;

public class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCampaignCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.HotelId == hotelId.Value && !c.IsDeleted, cancellationToken);

        if (campaign == null)
        {
            throw new EntityNotFoundException("Campaign", request.Id);
        }

        campaign.IsDeleted = true;
        campaign.UpdatedBy = _currentUserService.UserId;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}


public record GetCampaignByIdQuery(Guid Id) : IRequest<CampaignDto>;

public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, CampaignDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCampaignByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CampaignDto> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var c = await _context.Campaigns
            .Include(c => c.Recipients)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.HotelId == hotelId.Value && !c.IsDeleted, cancellationToken);

        if (c == null)
        {
            throw new EntityNotFoundException("Campaign", request.Id);
        }

        return new CampaignDto
        {
            Id = c.Id,
            HotelId = c.HotelId,
            Name = c.Name,
            Occasion = c.Occasion,
            MessageTemplate = c.MessageTemplate,
            Status = c.Status,
            ScheduledAt = c.ScheduledAt,
            SentAt = c.SentAt,
            TotalRecipients = c.Recipients.Count,
            SentCount = c.Recipients.Count(r => r.Status == RecipientStatus.Sent),
            FailedCount = c.Recipients.Count(r => r.Status == RecipientStatus.Failed),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
