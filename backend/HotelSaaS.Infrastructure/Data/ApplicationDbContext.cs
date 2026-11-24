using System.Reflection;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Common;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Enums;
using HotelSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUser;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUser = null) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<HotelSubscription> HotelSubscriptions => Set<HotelSubscription>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<GuestDocument> GuestDocuments => Set<GuestDocument>();
    public DbSet<GuestNote> GuestNotes => Set<GuestNote>();
    public DbSet<GuestConsent> GuestConsents => Set<GuestConsent>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<BookingGuest> BookingGuests => Set<BookingGuest>();
    public DbSet<CheckInSession> CheckInSessions => Set<CheckInSession>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<FavoriteGuest> FavoriteGuests => Set<FavoriteGuest>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignRecipient> CampaignRecipients => Set<CampaignRecipient>();
    public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();
    public DbSet<DeletionLog> DeletionLogs => Set<DeletionLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Pluralized table names mapping
        modelBuilder.Entity<Hotel>().ToTable("Hotels");
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<UserRole>().ToTable("UserRoles");
        modelBuilder.Entity<Permission>().ToTable("Permissions");
        modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
        modelBuilder.Entity<SubscriptionPlan>().ToTable("SubscriptionPlans");
        modelBuilder.Entity<HotelSubscription>().ToTable("HotelSubscriptions");
        modelBuilder.Entity<RoomType>().ToTable("RoomTypes");
        modelBuilder.Entity<Room>().ToTable("Rooms");
        modelBuilder.Entity<Guest>().ToTable("Guests");
        modelBuilder.Entity<GuestDocument>().ToTable("GuestDocuments");
        modelBuilder.Entity<GuestNote>().ToTable("GuestNotes");
        modelBuilder.Entity<GuestConsent>().ToTable("GuestConsents");
        modelBuilder.Entity<Booking>().ToTable("Bookings");
        modelBuilder.Entity<BookingRoom>().ToTable("BookingRooms");
        modelBuilder.Entity<BookingGuest>().ToTable("BookingGuests");
        modelBuilder.Entity<CheckInSession>().ToTable("CheckInSessions");
        modelBuilder.Entity<Invoice>().ToTable("Invoices");
        modelBuilder.Entity<InvoiceItem>().ToTable("InvoiceItems");
        modelBuilder.Entity<Payment>().ToTable("Payments");
        modelBuilder.Entity<FavoriteGuest>().ToTable("FavoriteGuests");
        modelBuilder.Entity<Campaign>().ToTable("Campaigns");
        modelBuilder.Entity<CampaignRecipient>().ToTable("CampaignRecipients");
        modelBuilder.Entity<MessageLog>().ToTable("MessageLogs");
        modelBuilder.Entity<Notification>().ToTable("Notifications");
        modelBuilder.Entity<RetentionPolicy>().ToTable("RetentionPolicies");
        modelBuilder.Entity<DeletionLog>().ToTable("DeletionLogs");
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");

        // Composite Keys
        modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Indexes
        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.PublicToken)
            .IsUnique();

        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.HotelId, b.BookingReference })
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => new { i.HotelId, i.InvoiceNumber })
            .IsUnique();

        modelBuilder.Entity<MessageLog>()
            .HasIndex(m => m.IdempotencyKey)
            .IsUnique();

        modelBuilder.Entity<BookingRoom>()
            .HasIndex(br => new { br.RoomId, br.CheckInDate, br.CheckOutDate });

        // Global Query Filters for Soft Delete
        modelBuilder.Entity<Hotel>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Guest>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GuestDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Room>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RoomType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Booking>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Campaign>().HasQueryFilter(e => !e.IsDeleted);

        // Configure Decimal precision: 18, 2 for all decimal columns
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Restrict delete behavior on all foreign keys to avoid SQL Server cycles/multiple cascade paths
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Explicit cascade configurations for owned child collections
        modelBuilder.Entity<InvoiceItem>()
            .HasOne(i => i.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CampaignRecipient>()
            .HasOne(r => r.Campaign)
            .WithMany(c => c.Recipients)
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
