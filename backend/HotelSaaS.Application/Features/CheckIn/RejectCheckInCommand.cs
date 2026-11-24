using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.CheckIn;

public record RejectCheckInCommand(Guid SessionId, string? Reason = null) : IRequest<bool>;

public class RejectCheckInCommandHandler : IRequestHandler<RejectCheckInCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RejectCheckInCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(RejectCheckInCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var session = await _context.CheckInSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.HotelId == hotelId.Value, cancellationToken);

        if (session == null)
        {
            throw new EntityNotFoundException("CheckInSession", request.SessionId);
        }

        session.Status = CheckInSessionStatus.Rejected;
        session.ReviewedBy = _currentUserService.UserId;
        session.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
