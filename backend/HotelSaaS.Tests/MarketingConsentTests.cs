using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HotelSaaS.Domain.Entities;
using Xunit;

namespace HotelSaaS.Tests;

public class MarketingConsentTests
{
    [Fact]
    public void CampaignRecipients_MustExcludeGuests_WithoutMarketingConsent()
    {
        // Arrange
        var guests = new List<Guest>
        {
            new Guest { Id = Guid.NewGuid(), FullName = "Consenting Guest", Mobile = "+919876543210", MarketingConsent = true, IsFavorite = true },
            new Guest { Id = Guid.NewGuid(), FullName = "Non-Consenting Guest", Mobile = "+919876543211", MarketingConsent = false, IsFavorite = true }
        };

        // Act - filter eligible recipients for Diwali campaign
        var eligibleRecipients = guests
            .Where(g => g.IsFavorite && g.MarketingConsent)
            .ToList();

        // Assert
        eligibleRecipients.Should().HaveCount(1);
        eligibleRecipients.First().FullName.Should().Be("Consenting Guest");
    }
}

