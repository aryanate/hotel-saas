using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HotelSaaS.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAllAsync(ApplicationDbContext context)
    {
        var hotels = await context.Hotels.Where(h => !h.IsDeleted).ToListAsync();
        foreach (var hotel in hotels)
        {
            await SeedHotelDataAsync(context, hotel.Id);
        }
    }

    public static async Task SeedHotelDataAsync(ApplicationDbContext context, Guid hotelId)
    {
        // 1. Check if room types already exist
        var existingRoomTypes = await context.RoomTypes.Where(rt => rt.HotelId == hotelId && !rt.IsDeleted).ToListAsync();
        RoomType standardType, deluxeType, superDeluxeType, executiveType, presidentialType;

        if (!existingRoomTypes.Any())
        {
            standardType = new RoomType
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                Name = "Standard Non-AC Room",
                Description = "Comfortable standard room with basic amenities, ceiling fan, and en-suite bathroom.",
                BasePrice = 1499.00m,
                MaxCapacity = 2,
                IsDeleted = false
            };

            deluxeType = new RoomType
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                Name = "Deluxe AC Room",
                Description = "Spacious air-conditioned room with king-size bed, Smart TV, high-speed Wi-Fi, and work desk.",
                BasePrice = 2799.00m,
                MaxCapacity = 3,
                IsDeleted = false
            };

            superDeluxeType = new RoomType
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                Name = "Super Deluxe AC Room",
                Description = "Premium city-view room with balcony, mini-refrigerator, tea/coffee maker, and luxury toiletries.",
                BasePrice = 4199.00m,
                MaxCapacity = 3,
                IsDeleted = false
            };

            executiveType = new RoomType
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                Name = "Executive Business Suite",
                Description = "Two-room suite with dedicated living area, ergonomic workstation, complimentary minibar, and express laundry.",
                BasePrice = 6499.00m,
                MaxCapacity = 4,
                IsDeleted = false
            };

            presidentialType = new RoomType
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                Name = "Presidential Family Suite",
                Description = "Ultra-luxury penthouse suite with panoramic view, master bedroom, twin guest room, dining area, and 24/7 butler service.",
                BasePrice = 10999.00m,
                MaxCapacity = 6,
                IsDeleted = false
            };

            context.RoomTypes.AddRange(standardType, deluxeType, superDeluxeType, executiveType, presidentialType);
            await context.SaveChangesAsync();
        }
        else
        {
            standardType = existingRoomTypes.FirstOrDefault(rt => rt.Name.Contains("Standard")) ?? existingRoomTypes.First();
            deluxeType = existingRoomTypes.FirstOrDefault(rt => rt.Name.Contains("Deluxe")) ?? existingRoomTypes.First();
            superDeluxeType = existingRoomTypes.FirstOrDefault(rt => rt.Name.Contains("Super")) ?? deluxeType;
            executiveType = existingRoomTypes.FirstOrDefault(rt => rt.Name.Contains("Executive")) ?? deluxeType;
            presidentialType = existingRoomTypes.FirstOrDefault(rt => rt.Name.Contains("Family") || rt.Name.Contains("Presidential")) ?? executiveType;
        }

        // 2. Rooms (25 rooms across 4 floors)
        var existingRooms = await context.Rooms.Where(r => r.HotelId == hotelId && !r.IsDeleted).ToListAsync();
        if (existingRooms.Count < 10)
        {
            var roomDefinitions = new[]
            {
                // Floor 1 (Standard & Deluxe)
                new { Num = "101", Type = standardType, Floor = 1, Status = RoomStatus.Occupied, Cap = 2, Price = 1499m, Desc = "Standard Non-AC Ground Floor" },
                new { Num = "102", Type = standardType, Floor = 1, Status = RoomStatus.Occupied, Cap = 2, Price = 1499m, Desc = "Standard Non-AC Garden View" },
                new { Num = "103", Type = standardType, Floor = 1, Status = RoomStatus.Available, Cap = 2, Price = 1499m, Desc = "Standard Non-AC Courtyard View" },
                new { Num = "104", Type = deluxeType, Floor = 1, Status = RoomStatus.Available, Cap = 3, Price = 2799m, Desc = "Deluxe AC Pool Facing" },
                new { Num = "105", Type = deluxeType, Floor = 1, Status = RoomStatus.Cleaning, Cap = 3, Price = 2799m, Desc = "Deluxe AC Pool Facing" },
                new { Num = "106", Type = deluxeType, Floor = 1, Status = RoomStatus.Available, Cap = 3, Price = 2799m, Desc = "Deluxe AC Near Lobby" },
                new { Num = "107", Type = deluxeType, Floor = 1, Status = RoomStatus.Available, Cap = 3, Price = 2799m, Desc = "Deluxe AC Quiet Corner" },

                // Floor 2 (Deluxe & Super Deluxe)
                new { Num = "201", Type = deluxeType, Floor = 2, Status = RoomStatus.Occupied, Cap = 3, Price = 2799m, Desc = "Deluxe AC Second Floor" },
                new { Num = "202", Type = deluxeType, Floor = 2, Status = RoomStatus.Available, Cap = 3, Price = 2799m, Desc = "Deluxe AC Second Floor" },
                new { Num = "203", Type = superDeluxeType, Floor = 2, Status = RoomStatus.Occupied, Cap = 3, Price = 4199m, Desc = "Super Deluxe with Private Balcony" },
                new { Num = "204", Type = superDeluxeType, Floor = 2, Status = RoomStatus.Available, Cap = 3, Price = 4199m, Desc = "Super Deluxe City View" },
                new { Num = "205", Type = superDeluxeType, Floor = 2, Status = RoomStatus.Cleaning, Cap = 3, Price = 4199m, Desc = "Super Deluxe City View" },
                new { Num = "206", Type = superDeluxeType, Floor = 2, Status = RoomStatus.Available, Cap = 3, Price = 4199m, Desc = "Super Deluxe Corner Suite" },
                new { Num = "207", Type = deluxeType, Floor = 2, Status = RoomStatus.Available, Cap = 3, Price = 2799m, Desc = "Deluxe AC East Wing" },

                // Floor 3 (Super Deluxe & Executive)
                new { Num = "301", Type = superDeluxeType, Floor = 3, Status = RoomStatus.Occupied, Cap = 3, Price = 4199m, Desc = "Super Deluxe Skyline View" },
                new { Num = "302", Type = superDeluxeType, Floor = 3, Status = RoomStatus.Occupied, Cap = 3, Price = 4199m, Desc = "Super Deluxe Skyline View" },
                new { Num = "303", Type = executiveType, Floor = 3, Status = RoomStatus.Available, Cap = 4, Price = 6499m, Desc = "Executive Business Suite A" },
                new { Num = "304", Type = executiveType, Floor = 3, Status = RoomStatus.Available, Cap = 4, Price = 6499m, Desc = "Executive Business Suite B" },
                new { Num = "305", Type = executiveType, Floor = 3, Status = RoomStatus.Maintenance, Cap = 4, Price = 6499m, Desc = "Executive Suite (AC Servicing)" },
                new { Num = "306", Type = superDeluxeType, Floor = 3, Status = RoomStatus.Available, Cap = 3, Price = 4199m, Desc = "Super Deluxe Quiet Zone" },

                // Floor 4 (Executive & Presidential)
                new { Num = "401", Type = executiveType, Floor = 4, Status = RoomStatus.Occupied, Cap = 4, Price = 6499m, Desc = "Executive Top Floor Suite" },
                new { Num = "402", Type = executiveType, Floor = 4, Status = RoomStatus.Available, Cap = 4, Price = 6499m, Desc = "Executive Top Floor Suite" },
                new { Num = "403", Type = presidentialType, Floor = 4, Status = RoomStatus.Available, Cap = 6, Price = 10999m, Desc = "Presidential Penthouse East" },
                new { Num = "404", Type = presidentialType, Floor = 4, Status = RoomStatus.Cleaning, Cap = 6, Price = 10999m, Desc = "Presidential Penthouse West" },
                new { Num = "405", Type = presidentialType, Floor = 4, Status = RoomStatus.Available, Cap = 6, Price = 10999m, Desc = "Grand Royal Family Suite" }
            };

            var newRooms = roomDefinitions.Select(d => new Room
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                RoomTypeId = d.Type.Id,
                RoomNumber = d.Num,
                Floor = d.Floor,
                Capacity = d.Cap,
                PricePerNight = d.Price,
                Description = d.Desc,
                Status = d.Status,
                IsDeleted = false
            }).ToList();

            context.Rooms.AddRange(newRooms);
            await context.SaveChangesAsync();
            existingRooms = newRooms;
        }

        // 3. Guests (8 realistic Indian profiles)
        var existingGuests = await context.Guests.Where(g => g.HotelId == hotelId && !g.IsDeleted).ToListAsync();
        if (existingGuests.Count < 5)
        {
            var guestsData = new[]
            {
                new { Name = "Amit Verma", Mobile = "9845012345", Email = "amit.verma@tcs.com", City = "Bengaluru", State = "Karnataka", Fav = true, Cons = true, Visits = 4, Spend = 24500m, Pref = "High floor, extra feather pillows, vegetarian breakfast" },
                new { Name = "Sneha Patil", Mobile = "9765432109", Email = "sneha.patil@infosys.com", City = "Pune", State = "Maharashtra", Fav = true, Cons = true, Visits = 3, Spend = 18200m, Pref = "Corner room, quiet area, late checkout requested" },
                new { Name = "Vikram Malhotra", Mobile = "9820198201", Email = "v.malhotra@hcl.com", City = "Mumbai", State = "Maharashtra", Fav = false, Cons = true, Visits = 2, Spend = 12998m, Pref = "Early check-in at 10 AM, airport taxi required" },
                new { Name = "Priya Iyer", Mobile = "9444012345", Email = "priya.iyer@gmail.com", City = "Chennai", State = "Tamil Nadu", Fav = true, Cons = true, Visits = 5, Spend = 38400m, Pref = "South Indian breakfast, daily newspaper, non-smoking" },
                new { Name = "Rohan Gupta", Mobile = "9810981098", Email = "rohan.g@startup.io", City = "New Delhi", State = "Delhi", Fav = false, Cons = false, Visits = 1, Spend = 5598m, Pref = "Desk lamp, fast Wi-Fi for Zoom meetings" },
                new { Name = "Ananya Sen", Mobile = "9830012345", Email = "ananya.sen@wipro.com", City = "Kolkata", State = "West Bengal", Fav = false, Cons = true, Visits = 2, Spend = 16800m, Pref = "Twin beds, extra towels" },
                new { Name = "Deepak Nair", Mobile = "9447012345", Email = "deepak.nair@cochin.org", City = "Kochi", State = "Kerala", Fav = false, Cons = true, Visits = 1, Spend = 8398m, Pref = "Ground floor or lift proximity" },
                new { Name = "Meera Joshi", Mobile = "9825012345", Email = "meera.joshi@adun.com", City = "Ahmedabad", State = "Gujarat", Fav = true, Cons = true, Visits = 6, Spend = 49500m, Pref = "Jain meal options, tea kit refill twice daily" }
            };

            var newGuests = new List<Guest>();
            foreach (var g in guestsData)
            {
                var guest = new Guest
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    FullName = g.Name,
                    Mobile = g.Mobile,
                    Email = g.Email,
                    Address = $"{g.City}, {g.State}, India",
                    TotalVisits = g.Visits,
                    TotalSpending = g.Spend,
                    LastVisitAt = DateTime.UtcNow.AddDays(-g.Visits * 5),
                    Preferences = g.Pref,
                    IsFavorite = g.Fav,
                    MarketingConsent = g.Cons,
                    MarketingConsentAt = g.Cons ? DateTime.UtcNow.AddDays(-30) : null,
                    IsDeleted = false
                };
                newGuests.Add(guest);
            }

            context.Guests.AddRange(newGuests);
            await context.SaveChangesAsync();
            existingGuests = newGuests;

            // Add Guest KYC Documents
            var docs = new List<GuestDocument>();
            foreach (var g in existingGuests)
            {
                docs.Add(new GuestDocument
                {
                    Id = Guid.NewGuid(),
                    GuestId = g.Id,
                    DocumentType = DocumentType.MaskedAadhaar,
                    DocumentNumber = $"XXXX-XXXX-{new Random().Next(1000, 9999)}",
                    StorageKey = $"kyc/{hotelId}/{g.Id}/aadhaar.pdf",
                    IsVerified = true,
                    VerifiedAt = DateTime.UtcNow.AddDays(-1)
                });
            }
            context.GuestDocuments.AddRange(docs);
            await context.SaveChangesAsync();
        }

        // 4. Bookings, Invoices & Payments
        var existingBookings = await context.Bookings.Where(b => b.HotelId == hotelId && !b.IsDeleted).ToListAsync();
        if (existingBookings.Count < 5 && existingGuests.Count >= 4 && existingRooms.Count >= 7)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            // 1) Active CheckedIn Bookings (Occupying Rooms 101, 102, 201, 203, 301, 302, 401)
            var occupiedRooms = existingRooms.Where(r => r.Status == RoomStatus.Occupied).ToList();
            var activeBookings = new List<Booking>();

            for (int i = 0; i < Math.Min(occupiedRooms.Count, existingGuests.Count); i++)
            {
                var room = occupiedRooms[i];
                var guest = existingGuests[i];
                var bId = Guid.NewGuid();
                var refCode = $"BK-{now.Year}-{1001 + i}";

                var booking = new Booking
                {
                    Id = bId,
                    HotelId = hotelId,
                    GuestId = guest.Id,
                    BookingReference = refCode,
                    Source = i % 2 == 0 ? BookingSource.QR : BookingSource.Online,
                    Status = BookingStatus.CheckedIn,
                    NumberOfAdults = room.Capacity,
                    NumberOfChildren = 0,
                    CheckInDate = today.AddDays(-1),
                    CheckOutDate = today.AddDays(2),
                    SpecialRequests = guest.Preferences,
                    IsDeleted = false
                };

                activeBookings.Add(booking);
            }
            context.Bookings.AddRange(activeBookings);
            await context.SaveChangesAsync();

            // Link BookingRooms
            for (int i = 0; i < activeBookings.Count; i++)
            {
                var b = activeBookings[i];
                var r = occupiedRooms[i];
                context.BookingRooms.Add(new BookingRoom
                {
                    BookingId = b.Id,
                    RoomId = r.Id,
                    PricePerNight = r.PricePerNight,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate
                });
            }
            await context.SaveChangesAsync();

            // 2) Confirmed Future Bookings (Arrivals today and tomorrow)
            var futureBookings = new List<Booking>();
            var availRooms = existingRooms.Where(r => r.Status == RoomStatus.Available).Take(3).ToList();
            for (int i = 0; i < availRooms.Count; i++)
            {
                var r = availRooms[i];
                var g = existingGuests[(i + 4) % existingGuests.Count];
                var b = new Booking
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    GuestId = g.Id,
                    BookingReference = $"BK-{now.Year}-{2001 + i}",
                    Source = BookingSource.Manual,
                    Status = BookingStatus.Confirmed,
                    NumberOfAdults = 2,
                    NumberOfChildren = 1,
                    CheckInDate = today, // Today check-in!
                    CheckOutDate = today.AddDays(2),
                    SpecialRequests = "Quiet room requested",
                    IsDeleted = false
                };
                futureBookings.Add(b);
            }
            context.Bookings.AddRange(futureBookings);
            await context.SaveChangesAsync();

            // 3) Historical CheckedOut Bookings (For past 7 days revenue trends)
            var pastBookings = new List<Booking>();
            var invoices = new List<Invoice>();
            var payments = new List<Payment>();

            for (int d = 1; d <= 7; d++)
            {
                var pastDate = today.AddDays(-d);
                var g = existingGuests[d % existingGuests.Count];
                var bId = Guid.NewGuid();
                var bRef = $"BK-{now.Year}-{3000 + d}";

                var b = new Booking
                {
                    Id = bId,
                    HotelId = hotelId,
                    GuestId = g.Id,
                    BookingReference = bRef,
                    Source = BookingSource.Online,
                    Status = BookingStatus.CheckedOut,
                    NumberOfAdults = 2,
                    NumberOfChildren = 0,
                    CheckInDate = pastDate.AddDays(-2),
                    CheckOutDate = pastDate,
                    IsDeleted = false
                };
                pastBookings.Add(b);

                // Create Invoice for past booking
                var invId = Guid.NewGuid();
                var roomCharge = 2799m * 2; // 2 nights
                var foodCharge = 850m;
                var subTotal = roomCharge + foodCharge;
                var roomGst = roomCharge * 0.12m; // 12% GST
                var foodGst = foodCharge * 0.05m; // 5% GST
                var totalTax = Math.Round(roomGst + foodGst, 2);
                var totalAmount = subTotal + totalTax;

                var inv = new Invoice
                {
                    Id = invId,
                    HotelId = hotelId,
                    BookingId = bId,
                    InvoiceNumber = $"INV-{now.Year}-{10000 + d}",
                    InvoiceDate = pastDate,
                    GuestName = g.FullName,
                    GuestMobile = g.Mobile,
                    SubTotal = subTotal,
                    TaxAmount = totalTax,
                    DiscountAmount = 0m,
                    TotalAmount = totalAmount,
                    AmountPaid = totalAmount,
                    Status = InvoiceStatus.Paid,
                    Notes = "Paid via UPI on checkout",
                    IsDeleted = false
                };
                invoices.Add(inv);

                // Create Payment
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    InvoiceId = invId,
                    Amount = totalAmount,
                    PaymentMethod = d % 2 == 0 ? PaymentMethod.UPI : PaymentMethod.Card,
                    PaymentDate = pastDate,
                    Reference = $"UPI-TXN-{new Random().Next(100000, 999999)}",
                    Notes = "Full settlement received at front desk"
                });
            }

            context.Bookings.AddRange(pastBookings);
            context.Invoices.AddRange(invoices);
            context.Payments.AddRange(payments);
            await context.SaveChangesAsync();

            // 4) Add Invoice Items
            var items = new List<InvoiceItem>();
            foreach (var inv in invoices)
            {
                items.Add(new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    InvoiceId = inv.Id,
                    Description = "Deluxe AC Room (2 Nights)",
                    Category = ItemCategory.Room,
                    Quantity = 2,
                    UnitPrice = 2799m,
                    TaxRate = 12m,
                    TaxAmount = Math.Round(2799m * 2 * 0.12m, 2),
                    TotalAmount = 2799m * 2 * 1.12m
                });

                items.Add(new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    InvoiceId = inv.Id,
                    Description = "Dining & Beverages",
                    Category = ItemCategory.Food,
                    Quantity = 1,
                    UnitPrice = 850m,
                    TaxRate = 5m,
                    TaxAmount = Math.Round(850m * 0.05m, 2),
                    TotalAmount = 850m * 1.05m
                });
            }
            context.InvoiceItems.AddRange(items);
            await context.SaveChangesAsync();

            // 5) Add Pending Invoices (Unpaid for pending payments KPI)
            if (activeBookings.Any())
            {
                var currentBooking = activeBookings.First();
                var unpaidInv = new Invoice
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    BookingId = currentBooking.Id,
                    InvoiceNumber = $"INV-{now.Year}-99999",
                    InvoiceDate = today,
                    GuestName = existingGuests.First().FullName,
                    GuestMobile = existingGuests.First().Mobile,
                    SubTotal = 5598m,
                    TaxAmount = 671.76m,
                    DiscountAmount = 0m,
                    TotalAmount = 6269.76m,
                    AmountPaid = 2000m,
                    Status = InvoiceStatus.PartiallyPaid,
                    Notes = "Advance of ₹2,000 paid at check-in. Balance pending on departure.",
                    IsDeleted = false
                };
                context.Invoices.Add(unpaidInv);
                await context.SaveChangesAsync();
            }

            // 6) Add Check-In Sessions for Reception Queue
            var sessions = new List<CheckInSession>
            {
                new CheckInSession
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    BookingId = futureBookings.First().Id,
                    Token = Guid.NewGuid().ToString("N").Substring(0, 16),
                    TokenExpiry = now.AddDays(1),
                    Status = CheckInSessionStatus.Submitted, // Ready for reception to approve!
                    SubmittedAt = now.AddMinutes(-25)
                },
                new CheckInSession
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    BookingId = futureBookings.Last().Id,
                    Token = Guid.NewGuid().ToString("N").Substring(0, 16),
                    TokenExpiry = now.AddDays(1),
                    Status = CheckInSessionStatus.Submitted, // Second submission waiting in queue!
                    SubmittedAt = now.AddMinutes(-10)
                }
            };
            context.CheckInSessions.AddRange(sessions);
            await context.SaveChangesAsync();
        }

        // 5. Retention Policies
        var existingPolicies = await context.RetentionPolicies.Where(p => p.HotelId == hotelId).ToListAsync();
        if (!existingPolicies.Any())
        {
            var defaultPolicies = new[]
            {
                new RetentionPolicy { Id = Guid.NewGuid(), HotelId = hotelId, RecordType = RetentionRecordType.GuestProfile, RetentionDays = 2555, IsLegalHold = false, Notes = "7 years for Indian Sarais Act / Police Foreigners compliance" },
                new RetentionPolicy { Id = Guid.NewGuid(), HotelId = hotelId, RecordType = RetentionRecordType.GuestDocument, RetentionDays = 2555, IsLegalHold = false, Notes = "Government photo ID documents (Aadhaar/Passport)" },
                new RetentionPolicy { Id = Guid.NewGuid(), HotelId = hotelId, RecordType = RetentionRecordType.InvoiceRecord, RetentionDays = 2920, IsLegalHold = false, Notes = "8 years mandatory statutory retention under CGST/SGST Rules" },
                new RetentionPolicy { Id = Guid.NewGuid(), HotelId = hotelId, RecordType = RetentionRecordType.BookingRecord, RetentionDays = 1095, IsLegalHold = false, Notes = "3 years operational booking history" },
                new RetentionPolicy { Id = Guid.NewGuid(), HotelId = hotelId, RecordType = RetentionRecordType.ConsentRecord, RetentionDays = 2555, IsLegalHold = false, Notes = "DPDP Act 2023 proof of marketing consent" },
                new RetentionPolicy { Id = Guid.NewGuid(), HotelId = hotelId, RecordType = RetentionRecordType.AuditRecord, RetentionDays = 365, IsLegalHold = false, Notes = "1 year security and access logs" }
            };
            context.RetentionPolicies.AddRange(defaultPolicies);
            await context.SaveChangesAsync();
        }
    }
}
