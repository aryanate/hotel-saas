
using HotelSaaS.API.Common;
using HotelSaaS.Application.Common.Models;
using HotelSaaS.Application.Features.Billing;
using HotelSaaS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSaaS.API.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Route("api/invoices")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BillingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceDto>>>> GetInvoices(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] string? search,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInvoicesQuery
        {
            Status = status,
            Search = search,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<InvoiceDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoiceById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<InvoiceDto>.Ok(result));
    }

    [HttpPost]
    [HttpPost("~/api/v1/invoices")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> CreateInvoiceDirect(
        [FromBody] CreateInvoiceDirectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand
        {
            BookingId = request.BookingId,
            Items = request.Items ?? new List<CreateInvoiceItemRequest>(),
            DiscountAmount = request.DiscountAmount,
            GuestGstin = request.GuestGstin,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Invoice created successfully."));
    }

    [HttpPost("~/api/v1/bookings/{bookingId:guid}/invoice")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> CreateInvoice(
        [FromRoute] Guid bookingId,
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand
        {
            BookingId = bookingId,
            Items = request.Items,
            DiscountAmount = request.DiscountAmount,
            GuestGstin = request.GuestGstin,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Invoice created successfully."));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var pdfBytes = await _mediator.Send(new GetInvoicePdfQuery(id), cancellationToken);
        return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
    }

    [HttpPost("{id:guid}/send-whatsapp")]
    public async Task<ActionResult<ApiResponse<bool>>> SendInvoiceWhatsApp(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendInvoiceWhatsAppCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Invoice sent via WhatsApp."));
    }

    [HttpPost("{id:guid}/send-email")]
    public async Task<ActionResult<ApiResponse<bool>>> SendInvoiceEmail(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendInvoiceEmailCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Invoice sent via Email."));
    }

    [HttpPost("{id:guid}/payments")]
    [HttpPut("{id:guid}/payment")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> RecordPayment(
        [FromRoute] Guid id,
        [FromBody] RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RecordPaymentCommand
        {
            InvoiceId = id,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Reference = request.Reference ?? request.TransactionReference,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<PaymentDto>.Ok(result, "Payment recorded successfully."));
    }

    [HttpGet("{id:guid}/payments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentDto>>>> GetInvoicePayments(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInvoicePaymentsQuery(id), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PaymentDto>>.Ok(result));
    }
}

public class CreateInvoiceDirectRequest
{
    public Guid BookingId { get; set; }
    public List<CreateInvoiceItemRequest> Items { get; set; } = new();
    public decimal DiscountAmount { get; set; }
    public string? GuestGstin { get; set; }
    public string? Notes { get; set; }
}

public class CreateInvoiceRequest
{
    public List<CreateInvoiceItemRequest> Items { get; set; } = new();
    public decimal DiscountAmount { get; set; }
    public string? GuestGstin { get; set; }
    public string? Notes { get; set; }
}

public class RecordPaymentRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? Reference { get; set; }
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
}
