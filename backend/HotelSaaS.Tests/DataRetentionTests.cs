using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using Xunit;

namespace HotelSaaS.Tests;

public class DataRetentionTests
{
    [Fact]
    public void Document_UnderLegalHold_MustNotBeDeleted()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            RecordType = RetentionRecordType.GuestDocument,
            RetentionDays = 90,
            IsLegalHold = true
        };

        var doc = new GuestDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = DocumentType.Passport,
            CreatedAt = DateTime.UtcNow.AddDays(-120), // Exceeded 90 days
            RetainUntil = DateTime.UtcNow.AddDays(-30)
        };

        // Deletion rule check
        bool isExpired = doc.RetainUntil.HasValue && doc.RetainUntil < DateTime.UtcNow;
        bool canDelete = isExpired && !policy.IsLegalHold;

        // Assert
        canDelete.Should().BeFalse("records under legal hold cannot be deleted even after retention period expires");
    }

    [Fact]
    public void Invoice_MustBeRetainedForStatutoryPeriod_EvenIfGuestDocumentsExpired()
    {
        // Indian GST requires retaining invoices for 72 months (6 years)
        var invoicePolicy = new RetentionPolicy
        {
            RecordType = RetentionRecordType.InvoiceRecord,
            RetentionDays = 365 * 6,
            IsLegalHold = false
        };

        var docPolicy = new RetentionPolicy
        {
            RecordType = RetentionRecordType.GuestDocument,
            RetentionDays = 90,
            IsLegalHold = false
        };

        invoicePolicy.RetentionDays.Should().BeGreaterThan(docPolicy.RetentionDays);
    }
}

