using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelSaaS.Tests;

public class TenantIsolationTests
{
    private ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HotelA_CannotAccess_HotelB_Guests()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var hotelAId = Guid.NewGuid();
        var hotelBId = Guid.NewGuid();

        using (var context = CreateInMemoryDbContext(dbName))
        {
            var guestA = new Guest
            {
                Id = Guid.NewGuid(),
                HotelId = hotelAId,
                FullName = "Guest Hotel A",
                Mobile = "+919876543210"
            };

            var guestB = new Guest
            {
                Id = Guid.NewGuid(),
                HotelId = hotelBId,
                FullName = "Guest Hotel B",
                Mobile = "+919876543211"
            };

            context.Guests.AddRange(guestA, guestB);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = CreateInMemoryDbContext(dbName))
        {
            var hotelAGuests = await context.Guests
                .Where(g => g.HotelId == hotelAId)
                .ToListAsync();

            hotelAGuests.Should().HaveCount(1);
            hotelAGuests.First().FullName.Should().Be("Guest Hotel A");
            hotelAGuests.Any(g => g.HotelId == hotelBId).Should().BeFalse();
        }
    }

    [Fact]
    public async Task HotelA_CannotAccess_HotelB_Invoices()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var hotelAId = Guid.NewGuid();
        var hotelBId = Guid.NewGuid();

        using (var context = CreateInMemoryDbContext(dbName))
        {
            var invoiceA = new Invoice
            {
                Id = Guid.NewGuid(),
                HotelId = hotelAId,
                InvoiceNumber = "INV-001",
                InvoiceDate = DateTime.UtcNow,
                GuestName = "Guest A",
                GuestMobile = "+919876543210",
                TotalAmount = 5000m
            };

            var invoiceB = new Invoice
            {
                Id = Guid.NewGuid(),
                HotelId = hotelBId,
                InvoiceNumber = "INV-002",
                InvoiceDate = DateTime.UtcNow,
                GuestName = "Guest B",
                GuestMobile = "+919876543211",
                TotalAmount = 8000m
            };

            context.Invoices.AddRange(invoiceA, invoiceB);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = CreateInMemoryDbContext(dbName))
        {
            var hotelAInvoices = await context.Invoices
                .Where(i => i.HotelId == hotelAId)
                .ToListAsync();

            hotelAInvoices.Should().HaveCount(1);
            hotelAInvoices.First().InvoiceNumber.Should().Be("INV-001");
            hotelAInvoices.Any(i => i.HotelId == hotelBId).Should().BeFalse();
        }
    }
}

