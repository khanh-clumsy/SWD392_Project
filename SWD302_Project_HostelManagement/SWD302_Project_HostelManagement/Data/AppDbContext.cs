using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<HostelOwner> HostelOwners { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<BookingRequest> BookingRequests { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Hostel> Hostels { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomUpdateLog> RoomUpdateLogs { get; set; }

    public virtual DbSet<ViolationReport> ViolationReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tenant entity configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.TenantId).HasName("PK_Tenant");
            entity.ToTable("Tenant");
            entity.HasIndex(e => e.Email, "UX_Tenant_Email").IsUnique();

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Active'")
                .HasColumnName("status");
            entity.Property(e => e.AvatarUrl)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.IdentityCard)
                .HasMaxLength(50)
                .HasColumnName("identity_card");

            entity.HasCheckConstraint("CK_Tenant_Status",
                "[status] IN ('Active', 'Inactive', 'Banned')");
        });

        // HostelOwner entity configuration
        modelBuilder.Entity<HostelOwner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK_HostelOwner");
            entity.ToTable("HostelOwner");
            entity.HasIndex(e => e.Email, "UX_HostelOwner_Email").IsUnique();

            entity.Property(e => e.OwnerId)
                .HasColumnName("owner_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Active'")
                .HasColumnName("status");
            entity.Property(e => e.AvatarUrl)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.BusinessLicense)
                .HasMaxLength(100)
                .HasColumnName("business_license");

            entity.HasCheckConstraint("CK_HostelOwner_Status",
                "[status] IN ('Active', 'Inactive', 'Banned')");
        });

        // Admin entity configuration
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK_Admin");
            entity.ToTable("Admin");
            entity.HasIndex(e => e.Email, "UX_Admin_Email").IsUnique();

            entity.Property(e => e.AdminId)
                .HasColumnName("admin_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Active'")
                .HasColumnName("status");
            entity.Property(e => e.AvatarUrl)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasCheckConstraint("CK_Admin_Status",
                "[status] IN ('Active', 'Inactive', 'Banned')");
        });

        // Hostel entity
        modelBuilder.Entity<Hostel>(entity =>
        {
            entity.HasKey(e => e.HostelId).HasName("PK_Hostel");
            entity.ToTable("Hostel");
            entity.HasIndex(e => e.OwnerId, "IX_Hostel_OwnerId");
            entity.HasIndex(e => e.Status, "IX_Hostel_Status");

            entity.Property(e => e.HostelId)
                .HasColumnName("hostel_id");
            entity.Property(e => e.OwnerId)
                .HasColumnName("owner_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Address)
                .HasColumnName("address");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.Description)
                .HasColumnName("description");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PendingApproval'")
                .HasColumnName("status");
            entity.Property(e => e.RejectReason)
                .HasColumnName("reject_reason");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_date");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Owner)
                .WithMany(o => o.Hostels)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("FK_Hostel_HostelOwner");
        });

        // Room entity
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK_Room");
            entity.ToTable("Room");
            entity.HasIndex(e => new { e.HostelId, e.RoomNumber }, "UX_Room_HostelId_RoomNumber").IsUnique();
            entity.HasIndex(e => e.HostelId, "IX_Room_HostelId");
            entity.HasIndex(e => e.OwnerId, "IX_Room_OwnerId");
            entity.HasIndex(e => e.Status, "IX_Room_Status");

            entity.Property(e => e.RoomId)
                .HasColumnName("room_id");
            entity.Property(e => e.HostelId)
                .HasColumnName("hostel_id");
            entity.Property(e => e.OwnerId)
                .HasColumnName("owner_id");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(20)
                .HasColumnName("room_number");
            entity.Property(e => e.RoomType)
                .HasMaxLength(50)
                .HasColumnName("room_type");
            entity.Property(e => e.Capacity)
                .HasColumnName("capacity");
            entity.Property(e => e.PricePerMonth)
                .HasPrecision(12, 2)
                .HasColumnName("price_per_month");
            entity.Property(e => e.Area)
                .HasPrecision(8, 2)
                .HasColumnName("area");
            entity.Property(e => e.Floor)
                .HasColumnName("floor");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'")
                .HasColumnName("status");
            entity.Property(e => e.Description)
                .HasColumnName("description");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Hostel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(d => d.HostelId)
                .HasConstraintName("FK_Room_Hostel");

            entity.HasOne(d => d.Owner)
                .WithMany()
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Room_HostelOwner");
        });

        // BookingRequest entity
        modelBuilder.Entity<BookingRequest>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK_BookingRequest");
            entity.ToTable("BookingRequest");
            entity.HasIndex(e => e.RoomId, "IX_BookingRequest_RoomId");
            entity.HasIndex(e => e.TenantId, "IX_BookingRequest_TenantId");
            entity.HasIndex(e => e.Status, "IX_BookingRequest_Status");

            entity.Property(e => e.BookingId)
                .HasColumnName("booking_id");
            entity.Property(e => e.RoomId)
                .HasColumnName("room_id");
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");
            entity.Property(e => e.RequestType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Booking'")
                .HasColumnName("request_type");
            entity.Property(e => e.StartDate)
                .HasColumnName("start_date");
            entity.Property(e => e.EndDate)
                .HasColumnName("end_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'")
                .HasColumnName("status");
            entity.Property(e => e.RejectReason)
                .HasColumnName("reject_reason");
            entity.Property(e => e.CancelledAt)
        .           HasColumnName("cancelled_at")
                 .IsRequired(false);
                
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_date");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Room)
                .WithMany(r => r.BookingRequests)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_BookingRequest_Room");

            entity.HasOne(d => d.Tenant)
                .WithMany(t => t.BookingRequests)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BookingRequest_Tenant");
        });

        // Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK_Notification");
            entity.ToTable("Notification");
            entity.HasIndex(e => e.BookingId, "IX_Notification_BookingId");
            entity.HasIndex(e => e.Status, "IX_Notification_Status");

            entity.Property(e => e.NotificationId)
                .HasColumnName("notification_id");
            entity.Property(e => e.BookingId)
                .HasColumnName("booking_id");
            entity.Property(e => e.RecipientEmail)
                .HasMaxLength(150)
                .HasColumnName("recipient_email");
            entity.Property(e => e.Subject)
                .HasMaxLength(300)
                .HasColumnName("subject");
            entity.Property(e => e.MessageContent)
                .HasColumnName("message_content");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'")
                .HasColumnName("status");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_at");
            entity.Property(e => e.SentAt)
                .HasColumnName("sent_at");

            entity.HasOne(d => d.BookingRequest)
                .WithMany(b => b.Notifications)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Notification_BookingRequest");
        });

        // PaymentTransaction entity
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK_PaymentTransaction");
            entity.ToTable("PaymentTransaction");
            entity.HasIndex(e => e.BookingId, "IX_PaymentTransaction_BookingId");

            entity.Property(e => e.TransactionId)
                .HasColumnName("transaction_id");
            entity.Property(e => e.BookingId)
                .HasColumnName("booking_id");
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.GatewayRef)
                .HasMaxLength(200)
                .HasColumnName("gateway_ref");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'")
                .HasColumnName("status");
            entity.Property(e => e.PaidAt)
                .HasColumnName("paid_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_at");

            entity.HasOne(d => d.BookingRequest)
                .WithMany(b => b.PaymentTransactions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_PaymentTransaction_BookingRequest");

            entity.HasOne(d => d.Tenant)
                .WithMany(t => t.PaymentTransactions)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PaymentTransaction_Tenant");
        });

        // Favorite entity
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK_Favorite");
            entity.ToTable("Favorite");
            entity.HasIndex(e => new { e.TenantId, e.HostelId }, "UX_Favorite_TenantId_HostelId").IsUnique();
            entity.HasIndex(e => e.TenantId, "IX_Favorite_TenantId");

            entity.Property(e => e.FavoriteId)
                .HasColumnName("favorite_id");
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");
            entity.Property(e => e.HostelId)
                .HasColumnName("hostel_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Tenant)
                .WithMany(t => t.Favorites)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Favorite_Tenant");

            entity.HasOne(d => d.Hostel)
                .WithMany(h => h.Favorites)
                .HasForeignKey(d => d.HostelId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Favorite_Hostel");
        });

        // Review entity
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK_Review");
            entity.ToTable("Review");
            entity.HasIndex(e => e.HostelId, "IX_Review_HostelId");

            entity.Property(e => e.ReviewId)
                .HasColumnName("review_id");
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");
            entity.Property(e => e.HostelId)
                .HasColumnName("hostel_id");
            entity.Property(e => e.BookingId)
                .HasColumnName("booking_id");
            entity.Property(e => e.Rating)
                .HasColumnName("rating");
            entity.Property(e => e.Comment)
                .HasColumnName("comment");
            entity.Property(e => e.OwnerReply)
                .HasColumnName("owner_reply");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Tenant)
                .WithMany(t => t.Reviews)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Review_Tenant");

            entity.HasOne(d => d.Hostel)
                .WithMany(h => h.Reviews)
                .HasForeignKey(d => d.HostelId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Review_Hostel");

            entity.HasOne(d => d.BookingRequest)
                .WithMany()
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Review_BookingRequest");
        });

        // ViolationReport entity
        modelBuilder.Entity<ViolationReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK_ViolationReport");
            entity.ToTable("ViolationReport");

            entity.Property(e => e.ReportId)
                .HasColumnName("report_id");
            entity.Property(e => e.ReporterTenantId)
                .HasColumnName("reporter_tenant_id");
            entity.Property(e => e.ReportedTenantId)
                .HasColumnName("reported_tenant_id");
            entity.Property(e => e.HostelId)
                .HasColumnName("hostel_id");
            entity.Property(e => e.Reason)
                .HasColumnName("reason");
            entity.Property(e => e.Evidence)
                .HasColumnName("evidence");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'")
                .HasColumnName("status");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("created_date");
            entity.Property(e => e.ResolvedDate)
                .HasColumnName("resolved_date");

            entity.HasOne(d => d.Reporter)
                .WithMany(t => t.ViolationReports)
                .HasForeignKey(d => d.ReporterTenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ViolationReport_Reporter");

            entity.HasOne(d => d.ReportedTenant)
                .WithMany(t => t.ReportedViolations)
                .HasForeignKey(d => d.ReportedTenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ViolationReport_ReportedTenant");

            entity.HasOne(d => d.Hostel)
                .WithMany()
                .HasForeignKey(d => d.HostelId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ViolationReport_Hostel");
        });

        // RoomUpdateLog entity
        modelBuilder.Entity<RoomUpdateLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_RoomUpdateLog");
            entity.ToTable("RoomUpdateLog");

            entity.Property(e => e.LogId)
                .HasColumnName("log_id");
            entity.Property(e => e.RoomId)
                .HasColumnName("room_id");
            entity.Property(e => e.BookingId)
                .HasColumnName("booking_id");
            entity.Property(e => e.ChangedByOwnerId)
                .HasColumnName("changed_by_owner_id");
            entity.Property(e => e.StatusBefore)
                .HasMaxLength(30)
                .HasColumnName("status_before");
            entity.Property(e => e.StatusAfter)
                .HasMaxLength(30)
                .HasColumnName("status_after");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnName("changed_at");

            entity.HasOne(d => d.Room)
                .WithMany(r => r.RoomUpdateLogs)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_RoomUpdateLog_Room");

            entity.HasOne(d => d.BookingRequest)
                .WithMany(b => b.RoomUpdateLogs)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_RoomUpdateLog_BookingRequest");

            entity.HasOne(d => d.ChangedBy)
                .WithMany()
                .HasForeignKey(d => d.ChangedByOwnerId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_RoomUpdateLog_ChangedBy");
        });

        // ===== CHECK CONSTRAINTS for SQL Server =====
        modelBuilder.Entity<Tenant>()
            .HasCheckConstraint("CK_Tenant_Status",
                "[status] IN ('Active', 'Inactive', 'Banned')");

        modelBuilder.Entity<HostelOwner>()
            .HasCheckConstraint("CK_HostelOwner_Status",
                "[status] IN ('Active', 'Inactive', 'Banned')");

        modelBuilder.Entity<Admin>()
            .HasCheckConstraint("CK_Admin_Status",
                "[status] IN ('Active', 'Inactive', 'Banned')");

        modelBuilder.Entity<Hostel>()
            .HasCheckConstraint("CK_Hostel_Status",
                "[status] IN ('PendingApproval', 'Approved', 'Rejected', 'Deleted')");

        modelBuilder.Entity<Room>()
            .HasCheckConstraint("CK_Room_Status",
                "[status] IN ('Available', 'Reserved', 'Occupied', 'Maintenance', 'Inactive')");

        modelBuilder.Entity<BookingRequest>()
            .HasCheckConstraint("CK_BookingRequest_Status",
                "[status] IN ('Pending', 'Approved', 'Rejected', 'Cancelled', 'PendingPayment', 'DepositPaid', 'Confirmed')");

        modelBuilder.Entity<BookingRequest>()
            .HasCheckConstraint("CK_BookingRequest_Type",
                "[request_type] IN ('Booking', 'Viewing')");

        modelBuilder.Entity<Notification>()
            .HasCheckConstraint("CK_Notification_Status",
                "[status] IN ('Pending', 'Sent', 'Failed')");

        modelBuilder.Entity<PaymentTransaction>()
            .HasCheckConstraint("CK_PaymentTransaction_Status",
                "[status] IN ('Pending', 'Success', 'Failed', 'Cancelled', 'VerificationFailed')");

        modelBuilder.Entity<ViolationReport>()
            .HasCheckConstraint("CK_ViolationReport_Status",
                "[status] IN ('Pending', 'Resolved', 'Dismissed')");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
