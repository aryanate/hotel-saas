using System.Security.Claims;

namespace HotelSaaS.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var hotelIdClaim = context.User.FindFirst("hotel_id")?.Value
                               ?? context.User.FindFirst("HotelId")?.Value;

            if (!string.IsNullOrEmpty(hotelIdClaim) && Guid.TryParse(hotelIdClaim, out var hotelGuid))
            {
                context.Items["HotelId"] = hotelGuid;
                _logger.LogDebug("Tenant context set to HotelId: {HotelId}", hotelGuid);
            }
        }

        await _next(context);
    }
}
