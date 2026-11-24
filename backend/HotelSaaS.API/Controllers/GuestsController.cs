using HotelSaaS.API.Common;
using HotelSaaS.Application.Common.Models;
using HotelSaaS.Application.Features.Bookings;
using HotelSaaS.Application.Features.Guests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/guests")]
[Route("api/guests")]
[Authorize]
public class GuestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GuestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<GuestDto>>>> GetGuests(
        [FromQuery] string? search,
        [FromQuery] bool? isFavorite,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetGuestsQuery
        {
            Search = search,
            IsFavorite = isFavorite,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<GuestDto>>.Ok(result));
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<ApiResponse<PagedResult<GuestDto>>>> GetFavoriteGuests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetFavoriteGuestsQuery { PageNumber = page, PageSize = pageSize }, cancellationToken);
        return Ok(ApiResponse<PagedResult<GuestDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GuestDetailDto>>> GetGuestById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGuestByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<GuestDetailDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GuestDto>>> UpdateGuest(
        [FromRoute] Guid id,
        [FromBody] UpdateGuestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGuestCommand
        {
            Id = id,
            FullName = request.FullName,
            Mobile = request.Mobile,
            Email = request.Email,
            Address = request.Address,
            Preferences = request.Preferences,
            MarketingConsent = request.MarketingConsent
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<GuestDto>.Ok(result, "Guest profile updated successfully."));
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BookingDto>>>> GetGuestHistory(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGuestHistoryQuery(id), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BookingDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/favorite")]
    public async Task<ActionResult<ApiResponse<object>>> AddFavorite(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleFavoriteCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { isFavorite = result }, "Guest favorite status updated."));
    }

    [HttpDelete("{id:guid}/favorite")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveFavorite(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleFavoriteCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { isFavorite = result }, "Guest removed from favorites."));
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<ApiResponse<GuestNoteDto>>> AddGuestNote(
        [FromRoute] Guid id,
        [FromBody] AddGuestNoteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddGuestNoteCommand
        {
            GuestId = id,
            Note = request.GetEffectiveNote()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<GuestNoteDto>.Ok(result, "Note added successfully."));
    }

    [HttpGet("{id:guid}/documents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GuestDocumentDto>>>> GetGuestDocuments(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGuestDocumentsQuery(id), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GuestDocumentDto>>.Ok(result));
    }

    [HttpGet("documents/{docId:guid}/url")]
    [HttpGet("{id:guid}/documents/{docId:guid}/url")]
    public async Task<ActionResult<ApiResponse<object>>> GetDocumentSignedUrl(
        [FromRoute] Guid docId,
        [FromRoute] Guid? id,
        CancellationToken cancellationToken)
    {
        var guestId = id ?? Guid.Empty;
        var result = await _mediator.Send(new GetDocumentUrlQuery(guestId, docId), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { signedUrl = result }));
    }
}

public class UpdateGuestRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Preferences { get; set; }
    public bool MarketingConsent { get; set; }
}

public class AddGuestNoteRequest
{
    public string Note { get; set; } = string.Empty;
    public string? NoteText { get; set; }

    public string GetEffectiveNote() => !string.IsNullOrEmpty(NoteText) ? NoteText : Note;
}
