using Microsoft.EntityFrameworkCore;
using HotelSaaS.Domain.Entities;

namespace HotelSaaS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Hotel> Hotels { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<HotelSubscription> HotelSubscriptions { get; }
    DbSet<RoomType> RoomTypes { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Guest> Guests { get; }
    DbSet<GuestDocument> GuestDocuments { get; }
    DbSet<GuestNote> GuestNotes { get; }
    DbSet<GuestConsent> GuestConsents { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingRoom> BookingRooms { get; }
    DbSet<BookingGuest> BookingGuests { get; }
    DbSet<CheckInSession> CheckInSessions { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<FavoriteGuest> FavoriteGuests { get; }
    DbSet<Campaign> Campaigns { get; }
    DbSet<CampaignRecipient> CampaignRecipients { get; }
    DbSet<MessageLog> MessageLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<RetentionPolicy> RetentionPolicies { get; }
    DbSet<DeletionLog> DeletionLogs { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
