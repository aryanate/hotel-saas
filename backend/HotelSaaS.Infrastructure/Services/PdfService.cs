using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Interfaces;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace HotelSaaS.Infrastructure.Services;

public class PdfService : IPdfService
{
    public Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, Hotel hotel)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"Invoice {invoice.InvoiceNumber}";
        document.Info.Subject = $"Hotel Invoice - {hotel.Name}";

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        // Fonts
        var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
        var subTitleFont = new XFont("Arial", 12, XFontStyle.Bold);
        var headerFont = new XFont("Arial", 10, XFontStyle.Bold);
        var bodyFont = new XFont("Arial", 9, XFontStyle.Regular);
        var boldBodyFont = new XFont("Arial", 9, XFontStyle.Bold);
        var smallFont = new XFont("Arial", 8, XFontStyle.Regular);

        // Brushes and Pens
        var primaryBrush = new XSolidBrush(XColor.FromArgb(30, 64, 175)); // Blue
        var textBrush = new XSolidBrush(XColor.FromArgb(31, 41, 55)); // Dark Gray
        var lightGrayBrush = new XSolidBrush(XColor.FromArgb(243, 244, 246));
        var borderPen = new XPen(XColor.FromArgb(209, 213, 219), 0.5);
        var headerPen = new XPen(XColor.FromArgb(30, 64, 175), 1.5);

        double margin = 40;
        double currentY = margin;
        double pageWidth = page.Width.Point;
        double contentWidth = pageWidth - (margin * 2);

        // Top Banner / Hotel Name
        gfx.DrawString(hotel.Name.ToUpper(), titleFont, primaryBrush, new XPoint(margin, currentY + 15));
        gfx.DrawString("TAX INVOICE", subTitleFont, primaryBrush, new XPoint(pageWidth - margin - 90, currentY + 15));
        currentY += 35;

        // Hotel Info (Left) and Invoice Info (Right)
        double hotelInfoX = margin;
        double invoiceInfoX = pageWidth - margin - 180;

        gfx.DrawString(hotel.Address, bodyFont, textBrush, new XPoint(hotelInfoX, currentY));
        gfx.DrawString($"Invoice #: {invoice.InvoiceNumber}", boldBodyFont, textBrush, new XPoint(invoiceInfoX, currentY));
        currentY += 14;

        gfx.DrawString($"{hotel.City}, {hotel.State} - {hotel.PinCode}", bodyFont, textBrush, new XPoint(hotelInfoX, currentY));
        gfx.DrawString($"Date: {invoice.InvoiceDate:dd MMM yyyy}", bodyFont, textBrush, new XPoint(invoiceInfoX, currentY));
        currentY += 14;

        gfx.DrawString($"Phone: {hotel.Mobile} | Email: {hotel.Email}", bodyFont, textBrush, new XPoint(hotelInfoX, currentY));
        gfx.DrawString($"Status: {invoice.Status}", boldBodyFont, primaryBrush, new XPoint(invoiceInfoX, currentY));
        currentY += 14;

        if (!string.IsNullOrWhiteSpace(hotel.GstIn))
        {
            gfx.DrawString($"GSTIN: {hotel.GstIn}", bodyFont, textBrush, new XPoint(hotelInfoX, currentY));
            currentY += 14;
        }

        currentY += 10;
        gfx.DrawLine(borderPen, margin, currentY, pageWidth - margin, currentY);
        currentY += 15;

        // Bill To Section
        gfx.DrawString("BILL TO", subTitleFont, primaryBrush, new XPoint(margin, currentY));
        currentY += 15;

        gfx.DrawString($"Guest Name: {invoice.GuestName}", boldBodyFont, textBrush, new XPoint(margin, currentY));
        currentY += 14;
        gfx.DrawString($"Mobile: {invoice.GuestMobile}", bodyFont, textBrush, new XPoint(margin, currentY));
        currentY += 14;

        if (!string.IsNullOrWhiteSpace(invoice.GuestGstin))
        {
            gfx.DrawString($"Guest GSTIN: {invoice.GuestGstin}", bodyFont, textBrush, new XPoint(margin, currentY));
            currentY += 14;
        }

        currentY += 15;

        // Items Table Header
        double tableTop = currentY;
        gfx.DrawRectangle(lightGrayBrush, margin, tableTop, contentWidth, 22);
        gfx.DrawLine(headerPen, margin, tableTop + 22, pageWidth - margin, tableTop + 22);

        double colDesc = margin + 8;
        double colCategory = margin + 220;
        double colQty = margin + 300;
        double colPrice = margin + 350;
        double colTax = margin + 420;
        double colTotal = margin + 480;

        gfx.DrawString("Description", headerFont, textBrush, new XPoint(colDesc, tableTop + 15));
        gfx.DrawString("Category", headerFont, textBrush, new XPoint(colCategory, tableTop + 15));
        gfx.DrawString("Qty", headerFont, textBrush, new XPoint(colQty, tableTop + 15));
        gfx.DrawString("Rate", headerFont, textBrush, new XPoint(colPrice, tableTop + 15));
        gfx.DrawString("Tax", headerFont, textBrush, new XPoint(colTax, tableTop + 15));
        gfx.DrawString("Total", headerFont, textBrush, new XPoint(colTotal, tableTop + 15));

        currentY = tableTop + 25;

        // Table Rows
        foreach (var item in invoice.Items)
        {
            currentY += 16;
            gfx.DrawString(item.Description, bodyFont, textBrush, new XPoint(colDesc, currentY));
            gfx.DrawString(item.Category.ToString(), smallFont, textBrush, new XPoint(colCategory, currentY));
            gfx.DrawString(item.Quantity.ToString("0.##"), bodyFont, textBrush, new XPoint(colQty, currentY));
            gfx.DrawString(item.UnitPrice.ToString("N2"), bodyFont, textBrush, new XPoint(colPrice, currentY));
            gfx.DrawString(item.TaxAmount.ToString("N2"), bodyFont, textBrush, new XPoint(colTax, currentY));
            gfx.DrawString(item.TotalAmount.ToString("N2"), boldBodyFont, textBrush, new XPoint(colTotal, currentY));

            currentY += 6;
            gfx.DrawLine(borderPen, margin, currentY, pageWidth - margin, currentY);
        }

        currentY += 25;

        // Summary Breakdown (Right-aligned)
        double summaryX = pageWidth - margin - 220;
        double summaryValX = pageWidth - margin - 50;

        gfx.DrawString("Subtotal:", bodyFont, textBrush, new XPoint(summaryX, currentY));
        gfx.DrawString($"{hotel.Currency} {invoice.SubTotal:N2}", bodyFont, textBrush, new XPoint(summaryValX - 20, currentY));
        currentY += 16;

        gfx.DrawString("Tax:", bodyFont, textBrush, new XPoint(summaryX, currentY));
        gfx.DrawString($"{hotel.Currency} {invoice.TaxAmount:N2}", bodyFont, textBrush, new XPoint(summaryValX - 20, currentY));
        currentY += 16;

        if (invoice.DiscountAmount > 0)
        {
            gfx.DrawString("Discount:", bodyFont, textBrush, new XPoint(summaryX, currentY));
            gfx.DrawString($"- {hotel.Currency} {invoice.DiscountAmount:N2}", bodyFont, textBrush, new XPoint(summaryValX - 20, currentY));
            currentY += 16;
        }

        gfx.DrawLine(borderPen, summaryX, currentY - 4, pageWidth - margin, currentY - 4);

        gfx.DrawString("Total Amount:", boldBodyFont, primaryBrush, new XPoint(summaryX, currentY + 10));
        gfx.DrawString($"{hotel.Currency} {invoice.TotalAmount:N2}", boldBodyFont, primaryBrush, new XPoint(summaryValX - 20, currentY + 10));
        currentY += 24;

        gfx.DrawString("Amount Paid:", bodyFont, textBrush, new XPoint(summaryX, currentY));
        gfx.DrawString($"{hotel.Currency} {invoice.AmountPaid:N2}", bodyFont, textBrush, new XPoint(summaryValX - 20, currentY));
        currentY += 16;

        gfx.DrawString("Balance Due:", boldBodyFont, textBrush, new XPoint(summaryX, currentY));
        gfx.DrawString($"{hotel.Currency} {invoice.OutstandingBalance:N2}", boldBodyFont, textBrush, new XPoint(summaryValX - 20, currentY));
        currentY += 30;

        // Footer / Notes
        if (!string.IsNullOrWhiteSpace(invoice.Notes))
        {
            gfx.DrawString($"Notes: {invoice.Notes}", smallFont, textBrush, new XPoint(margin, currentY));
            currentY += 16;
        }

        double footerY = page.Height.Point - margin;
        gfx.DrawLine(borderPen, margin, footerY - 15, pageWidth - margin, footerY - 15);
        gfx.DrawString("Thank you for your stay! This is a computer-generated invoice.", smallFont, textBrush, new XPoint(margin, footerY));

        using var memoryStream = new MemoryStream();
        document.Save(memoryStream, false);
        return Task.FromResult(memoryStream.ToArray());
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var hotel = invoice.Booking?.Hotel ?? new Hotel
        {
            Id = invoice.HotelId,
            Name = "Hotel Management",
            Address = "Property Address",
            City = "City",
            State = "State",
            PinCode = "",
            Email = "info@hotel.com",
            Mobile = "+910000000000",
            Currency = "INR"
        };

        return await GenerateInvoicePdfAsync(invoice, hotel);
    }
}
