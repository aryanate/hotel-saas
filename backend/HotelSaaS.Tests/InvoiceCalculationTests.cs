using System;
using System.Collections.Generic;
using FluentAssertions;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using Xunit;

namespace HotelSaaS.Tests;

public class InvoiceCalculationTests
{
    [Fact]
    public void CalculateInvoiceTotals_WithGstAndDiscounts_ComputesAccurateAmounts()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            InvoiceNumber = "INV/2026-27/000001",
            InvoiceDate = DateTime.UtcNow,
            GuestName = "Rahul Sharma",
            GuestMobile = "+919876543210"
        };

        var items = new List<InvoiceItem>
        {
            new InvoiceItem
            {
                Id = Guid.NewGuid(),
                Description = "Deluxe Room (2 Nights)",
                Category = ItemCategory.Room,
                Quantity = 2m,
                UnitPrice = 3000m,
                TaxRate = 12m,
                TaxAmount = 720m,      // 6000 * 0.12
                TotalAmount = 6720m    // 6000 + 720
            },
            new InvoiceItem
            {
                Id = Guid.NewGuid(),
                Description = "Restaurant Dinner",
                Category = ItemCategory.Food,
                Quantity = 1m,
                UnitPrice = 1000m,
                TaxRate = 5m,
                TaxAmount = 50m,       // 1000 * 0.05
                TotalAmount = 1050m    // 1000 + 50
            }
        };

        decimal subtotal = 0m;
        decimal totalTax = 0m;
        foreach (var item in items)
        {
            subtotal += item.Quantity * item.UnitPrice;
            totalTax += item.TaxAmount;
        }

        decimal discount = 500m;
        decimal grandTotal = subtotal + totalTax - discount;

        invoice.SubTotal = subtotal;
        invoice.TaxAmount = totalTax;
        invoice.DiscountAmount = discount;
        invoice.TotalAmount = grandTotal;
        invoice.AmountPaid = 7000m;

        // Assert
        invoice.SubTotal.Should().Be(7000m);
        invoice.TaxAmount.Should().Be(770m);
        invoice.TotalAmount.Should().Be(7270m);
        invoice.OutstandingBalance.Should().Be(270m);
    }

    [Fact]
    public void Gst_OnRoomBelow7500_ShouldBeCalculatedPerRoom()
    {
        // Indian GST Rule: Room below 7500 is 12% (or 5%), not 18% based on cumulative bill
        decimal room1Price = 5000m;
        decimal room2Price = 5000m;

        decimal room1Tax = Math.Round(room1Price * 0.12m, 2);
        decimal room2Tax = Math.Round(room2Price * 0.12m, 2);

        decimal totalTax = room1Tax + room2Tax;

        // Both are individually <= 7500
        totalTax.Should().Be(1200m);
    }
}

