using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.Guests;

public record AddGuestNoteCommand : IRequest<GuestNoteDto>
{
    public Guid GuestId { get; init; }
    public string Note { get; init; } = string.Empty;
}

public class AddGuestNoteCommandValidator : AbstractValidator<AddGuestNoteCommand>
{
    public AddGuestNoteCommandValidator()
    {
        RuleFor(x => x.GuestId).NotEmpty();
        RuleFor(x => x.Note).NotEmpty().MaximumLength(1000);
    }
}

public class AddGuestNoteCommandHandler : IRequestHandler<AddGuestNoteCommand, GuestNoteDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddGuestNoteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GuestNoteDto> Handle(AddGuestNoteCommand request, CancellationToken cancellationToken)
    {
        var hotelId = _currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            throw new TenantNotFoundException("No hotel context found for current user.");
        }

        var guest = await _context.Guests
            .FirstOrDefaultAsync(g => g.Id == request.GuestId && g.HotelId == hotelId.Value && !g.IsDeleted, cancellationToken);

        if (guest == null)
        {
            throw new EntityNotFoundException("Guest", request.GuestId);
        }

        var note = new GuestNote
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId.Value,
            GuestId = guest.Id,
            Note = request.Note,
            CreatedBy = _currentUserService.UserId
        };

        _context.GuestNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return new GuestNoteDto
        {
            Id = note.Id,
            Note = note.Note,
            CreatedBy = note.CreatedBy,
            CreatedAt = note.CreatedAt
        };
    }
}
