using HotelSaaS.API.Common;
using HotelSaaS.Application.Features.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(result));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<RevenueReportDto>>> GetRevenueReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetRevenueReportQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<RevenueReportDto>.Ok(result));
    }

    [HttpGet("occupancy")]
    public async Task<ActionResult<ApiResponse<OccupancyReportDto>>> GetOccupancyReport(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOccupancyReportQuery(), cancellationToken);
        return Ok(ApiResponse<OccupancyReportDto>.Ok(result));
    }

    [HttpGet("guests")]
    public async Task<ActionResult<ApiResponse<GuestReportDto>>> GetGuestReport(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGuestReportQuery(), cancellationToken);
        return Ok(ApiResponse<GuestReportDto>.Ok(result));
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<ApiResponse<InvoiceReportDto>>> GetInvoiceReport(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInvoiceReportQuery(), cancellationToken);
        return Ok(ApiResponse<InvoiceReportDto>.Ok(result));
    }

    [HttpGet("gst")]
    public async Task<ActionResult<ApiResponse<GstReportDto>>> GetGstReport(
        [FromQuery] int? month,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var query = new GetGstReportQuery(month ?? now.Month, year ?? now.Year);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<GstReportDto>.Ok(result));
    }
}
