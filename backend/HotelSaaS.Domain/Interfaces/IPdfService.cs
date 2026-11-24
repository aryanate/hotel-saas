using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Domain.Interfaces;

/// <summary>
/// Provides PDF generation capabilities for hotel documents.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a PDF for a given invoice.
    /// </summary>
    /// <param name="invoice">The invoice entity to render.</param>
    /// <param name="hotel">The hotel entity for branding and details.</param>
    /// <returns>The PDF content as a byte array.</returns>
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, Hotel hotel);

    /// <summary>
    /// Generates a PDF for a given invoice using available context.
    /// </summary>
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
