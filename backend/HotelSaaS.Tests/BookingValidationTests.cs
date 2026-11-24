using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using Xunit;

namespace HotelSaaS.Tests;

public class BookingValidationTests
{
    [Fact]
    public void OverlappingBookings_ForSameRoom_ShouldBeDetected()
    {
        // Room 101 booked from Nov 15 to Nov 18
        var existingBooking = new BookingRoom
        {
            RoomId = Guid.NewGuid(),
            CheckInDate = new DateTime(2026, 11, 15),
            CheckOutDate = new DateTime(2026, 11, 18)
        };

        // Attempt new booking from Nov 17 to Nov 20 (overlapping)
        var newCheckIn = new DateTime(2026, 11, 17);
        var newCheckOut = new DateTime(2026, 11, 20);

        bool isConflict = (newCheckIn < existingBooking.CheckOutDate) && (newCheckOut > existingBooking.CheckInDate);

        isConflict.Should().BeTrue("overlapping dates on the same room must be flagged as conflict");
    }

    [Fact]
    public void NonOverlappingBookings_ForSameRoom_ShouldBeAllowed()
    {
        // Room 101 booked from Nov 15 to Nov 18
        var existingBooking = new BookingRoom
        {
            RoomId = Guid.NewGuid(),
            CheckInDate = new DateTime(2026, 11, 15),
            CheckOutDate = new DateTime(2026, 11, 18)
        };

        // Same room booked from Nov 18 to Nov 21 (checkout day matches check-in day)
        var newCheckIn = new DateTime(2026, 11, 18);
        var newCheckOut = new DateTime(2026, 11, 21);

        bool isConflict = (newCheckIn < existingBooking.CheckOutDate) && (newCheckOut > existingBooking.CheckInDate);

        isConflict.Should().BeFalse("back-to-back booking on check-out date is permissible");
    }
}

