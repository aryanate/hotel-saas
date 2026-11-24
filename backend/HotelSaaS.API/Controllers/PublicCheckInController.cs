using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.CheckIn;
using HotelSaaS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/public/checkin")]
[Route("api/public/checkin")]
[Route("api/v1/checkin/public")]
[Route("api/checkin/public")]
[AllowAnonymous]
public class PublicCheckInController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicCheckInController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{token}")]
    public async Task<ActionResult<ApiResponse<PublicCheckInInfoDto>>> GetCheckInPage(
        [FromRoute] string token,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCheckInPageQuery(token), cancellationToken);
        return Ok(ApiResponse<PublicCheckInInfoDto>.Ok(result));
    }

    [HttpGet("{token}/qr")]
    public async Task<IActionResult> GetPublicCheckInQr(
        [FromRoute] string token,
        [FromQuery] string? host,
        [FromServices] HotelSaaS.Domain.Interfaces.IQrCodeService qrCodeService,
        [FromServices] IConfiguration config,
        CancellationToken cancellationToken)
    {
        var frontendUrl = !string.IsNullOrWhiteSpace(host) ? host : (config["Frontend:BaseUrl"] ?? "http://localhost:5173");
        var fullUrl = $"{frontendUrl.TrimEnd('/')}/checkin/{token}";
        var qrBytes = await qrCodeService.GenerateQrCodeAsync(fullUrl, 400);
        return File(qrBytes, "image/png", $"checkin-{token}.png");
    }

    [HttpPost("{token}")]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitCheckIn(
        [FromRoute] string token,
        [FromBody] SubmitCheckInJsonRequest request,
        CancellationToken cancellationToken)
    {
        var docType = request.DocumentType ?? DocumentType.Other;
        if (!string.IsNullOrEmpty(request.IdType) && Enum.TryParse<DocumentType>(request.IdType, true, out var parsedType))
        {
            docType = parsedType;
        }

        var command = new SubmitCheckInCommand
        {
            Token = token,
            FullName = request.FullName,
            Mobile = request.Mobile,
            Email = request.Email,
            Address = request.Address,
            DocumentType = docType,
            DocumentNumber = request.DocumentNumber ?? request.IdNumber,
            DocumentStream = null,
            DocumentFileName = null,
            MarketingConsent = request.MarketingConsent
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Check-in submitted successfully."));
    }

    [HttpPost("{token}/upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitCheckInForm(
        [FromRoute] string token,
        [FromForm] SubmitCheckInRequest request,
        CancellationToken cancellationToken)
    {
        Stream? docStream = null;
        string? docFileName = null;

        if (request.DocumentFile != null && request.DocumentFile.Length > 0)
        {
            docStream = request.DocumentFile.OpenReadStream();
            docFileName = request.DocumentFile.FileName;
        }

        var command = new SubmitCheckInCommand
        {
            Token = token,
            FullName = request.FullName,
            Mobile = request.Mobile,
            Email = request.Email,
            Address = request.Address,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            DocumentStream = docStream,
            DocumentFileName = docFileName,
            MarketingConsent = request.MarketingConsent
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Check-in submitted successfully."));
    }

    [HttpGet("{token}/status")]
    public async Task<ActionResult<ApiResponse<CheckInSessionDto>>> GetCheckInStatus(
        [FromRoute] string token,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCheckInStatusQuery(token), cancellationToken);
        return Ok(ApiResponse<CheckInSessionDto>.Ok(result));
    }
}

public class SubmitCheckInJsonRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdType { get; set; }
    public DocumentType? DocumentType { get; set; }
    public string? IdNumber { get; set; }
    public string? DocumentNumber { get; set; }
    public bool MarketingConsent { get; set; }
}

public class SubmitCheckInRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DocumentType DocumentType { get; set; } = DocumentType.Other;
    public string? DocumentNumber { get; set; }
    public IFormFile? DocumentFile { get; set; }
    public bool MarketingConsent { get; set; }
}
