using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.Hotels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/hotels")]
[Route("api/hotels")]
[Authorize]
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HotelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<HotelDto>>> GetMyHotel(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyHotelQuery(), cancellationToken);
        return Ok(ApiResponse<HotelDto>.Ok(result));
    }

    [HttpPut("me")]
    [HttpPut("my")]
    public async Task<ActionResult<ApiResponse<HotelDto>>> UpdateMyHotel(
        [FromBody] UpdateHotelCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<HotelDto>.Ok(result, "Hotel profile updated successfully."));
    }

    [HttpGet("me/qr")]
    public async Task<IActionResult> GetHotelQr(CancellationToken cancellationToken)
    {
        var qrBytes = await _mediator.Send(new GetHotelQrQuery(), cancellationToken);
        return File(qrBytes, "image/png", "hotel-qr.png");
    }

    [HttpGet("qrcode")]
    [HttpGet("me/qrcode")]
    public async Task<ActionResult<ApiResponse<HotelQrInfoDto>>> GetHotelQrInfo(
        [FromQuery] string? hostUrl,
        [FromServices] IConfiguration config,
        CancellationToken cancellationToken)
    {
        var localIp = GetLocalIpAddress();
        var defaultUrl = localIp != "localhost" ? $"http://{localIp}:5173" : (config["Frontend:BaseUrl"] ?? "http://localhost:5173");
        var frontendUrl = !string.IsNullOrWhiteSpace(hostUrl) ? hostUrl : defaultUrl;

        var hotel = await _mediator.Send(new GetMyHotelQuery(), cancellationToken);
        var qrBytes = await _mediator.Send(new GetHotelQrQuery(frontendUrl), cancellationToken);
        var base64 = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";
        var fullCheckInUrl = $"{frontendUrl.TrimEnd('/')}/checkin/{hotel.PublicToken}";

        var dto = new HotelQrInfoDto
        {
            QrCodeUrl = $"/api/public/checkin/{hotel.PublicToken}/qr?host={Uri.EscapeDataString(frontendUrl)}",
            QrCodeBase64 = base64,
            CheckInUrl = fullCheckInUrl,
            PublicToken = hotel.PublicToken.ToString(),
            LocalIp = localIp
        };
        return Ok(ApiResponse<HotelQrInfoDto>.Ok(dto));
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            using var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            if (socket.LocalEndPoint is System.Net.IPEndPoint endPoint)
            {
                return endPoint.Address.ToString();
            }
        }
        catch { }

        try
        {
            var ip = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                            n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                                     !System.Net.IPAddress.IsLoopback(a.Address))?.Address.ToString();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }
        catch { }

        return "localhost";
    }

    [HttpPost("me/seed")]
    [HttpPost("seed")]
    public async Task<ActionResult<ApiResponse<bool>>> SeedHotelDemoData(
        [FromServices] HotelSaaS.Infrastructure.Data.ApplicationDbContext context,
        [FromServices] HotelSaaS.Domain.Interfaces.ICurrentUserService currentUserService)
    {
        var hotelId = currentUserService.HotelId;
        if (!hotelId.HasValue)
        {
            return BadRequest(ApiResponse<bool>.Fail("Hotel context not found.", "NO_TENANT"));
        }

        await HotelSaaS.Infrastructure.Data.DatabaseSeeder.SeedHotelDataAsync(context, hotelId.Value);
        return Ok(ApiResponse<bool>.Ok(true, "Demo data successfully seeded for this hotel."));
    }

    [HttpPost("me/logo")]
    [HttpPost("logo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<string>>> UploadLogo(
        [FromForm] UploadLogoRequest request,
        CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.Fail("File is empty or not provided.", "INVALID_FILE"));
        }

        using var stream = file.OpenReadStream();
        var command = new UploadLogoCommand
        {
            ContentStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var logoUrl = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<string>.Ok(logoUrl, "Logo uploaded successfully."));
    }
}

public class UploadLogoRequest
{
    public IFormFile File { get; set; } = null!;
}

public class HotelQrInfoDto
{
    public string QrCodeUrl { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
    public string CheckInUrl { get; set; } = string.Empty;
    public string PublicToken { get; set; } = string.Empty;
    public string LocalIp { get; set; } = string.Empty;
}
