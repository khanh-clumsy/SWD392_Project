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

    public virtual DbSet<BookingRequest> BookingRequests { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Hostel> Hostels { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomUpdateLog> RoomUpdateLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<ViolationReport> ViolationReports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.qayckpwfzblnbfthzebi;Password=phamgiakhanh123h;SSL Mode=Require;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<BookingRequest>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("BookingRequest_pkey");

            entity.ToTable("BookingRequest");

            entity.HasIndex(e => e.RoomId, "idx_booking_room");

            entity.HasIndex(e => e.Status, "idx_booking_status");

            entity.HasIndex(e => e.TenantId, "idx_booking_tenant");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.RejectReason).HasColumnName("reject_reason");
            entity.Property(e => e.RequestType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Booking'::character varying")
                .HasColumnName("request_type");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Room).WithMany(p => p.BookingRequests)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("BookingRequest_room_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.BookingRequests)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("BookingRequest_tenant_id_fkey");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("Favorite_pkey");

            entity.ToTable("Favorite");

            entity.HasIndex(e => new { e.TenantId, e.HostelId }, "Favorite_tenant_id_hostel_id_key").IsUnique();

            entity.HasIndex(e => e.TenantId, "idx_favorite_tenant");

            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.HostelId).HasColumnName("hostel_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.HasOne(d => d.Hostel).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.HostelId)
                .HasConstraintName("Favorite_hostel_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("Favorite_tenant_id_fkey");
        });

        modelBuilder.Entity<Hostel>(entity =>
        {
            entity.HasKey(e => e.HostelId).HasName("Hostel_pkey");

            entity.ToTable("Hostel");

            entity.HasIndex(e => e.OwnerId, "idx_hostel_owner");

            entity.HasIndex(e => e.Status, "idx_hostel_status");

            entity.Property(e => e.HostelId).HasColumnName("hostel_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.RejectReason).HasColumnName("reject_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'PendingApproval'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Owner).WithMany(p => p.Hostels)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("Hostel_owner_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("Notification_pkey");

            entity.ToTable("Notification");

            entity.HasIndex(e => e.BookingId, "idx_notif_booking");

            entity.HasIndex(e => e.Status, "idx_notif_status");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.MessageContent).HasColumnName("message_content");
            entity.Property(e => e.RecipientEmail)
                .HasMaxLength(150)
                .HasColumnName("recipient_email");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(300)
                .HasColumnName("subject");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.Booking).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Notification_booking_id_fkey");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PaymentTransaction_pkey");

            entity.ToTable("PaymentTransaction");

            entity.HasIndex(e => e.BookingId, "idx_payment_booking");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GatewayRef)
                .HasMaxLength(200)
                .HasColumnName("gateway_ref");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("PaymentTransaction_booking_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("PaymentTransaction_tenant_id_fkey");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("Review_pkey");

            entity.ToTable("Review");

            entity.HasIndex(e => e.HostelId, "idx_review_hostel");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.HostelId).HasColumnName("hostel_id");
            entity.Property(e => e.OwnerReply).HasColumnName("owner_reply");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Review_booking_id_fkey");

            entity.HasOne(d => d.Hostel).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.HostelId)
                .HasConstraintName("Review_hostel_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("Review_tenant_id_fkey");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("Room_pkey");

            entity.ToTable("Room");

            entity.HasIndex(e => new { e.HostelId, e.RoomNumber }, "Room_hostel_id_room_number_key").IsUnique();

            entity.HasIndex(e => e.HostelId, "idx_room_hostel");

            entity.HasIndex(e => e.OwnerId, "idx_room_owner");

            entity.HasIndex(e => e.Status, "idx_room_status");

            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.Area)
                .HasPrecision(8, 2)
                .HasColumnName("area");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Floor).HasColumnName("floor");
            entity.Property(e => e.HostelId).HasColumnName("hostel_id");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.PricePerMonth)
                .HasPrecision(12, 2)
                .HasColumnName("price_per_month");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(20)
                .HasColumnName("room_number");
            entity.Property(e => e.RoomType)
                .HasMaxLength(50)
                .HasColumnName("room_type");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Hostel).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.HostelId)
                .HasConstraintName("Room_hostel_id_fkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("Room_owner_id_fkey");
        });

        modelBuilder.Entity<RoomUpdateLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("RoomUpdateLog_pkey");

            entity.ToTable("RoomUpdateLog");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.StatusAfter)
                .HasMaxLength(30)
                .HasColumnName("status_after");
            entity.Property(e => e.StatusBefore)
                .HasMaxLength(30)
                .HasColumnName("status_before");

            entity.HasOne(d => d.Booking).WithMany(p => p.RoomUpdateLogs)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("RoomUpdateLog_booking_id_fkey");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.RoomUpdateLogs)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("RoomUpdateLog_changed_by_fkey");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomUpdateLogs)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("RoomUpdateLog_room_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Active'::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<ViolationReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("ViolationReport_pkey");

            entity.ToTable("ViolationReport");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.Evidence).HasColumnName("evidence");
            entity.Property(e => e.HostelId).HasColumnName("hostel_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.ReportedUserId).HasColumnName("reported_user_id");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.ResolvedDate).HasColumnName("resolved_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Hostel).WithMany(p => p.ViolationReports)
                .HasForeignKey(d => d.HostelId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ViolationReport_hostel_id_fkey");

            entity.HasOne(d => d.ReportedUser).WithMany(p => p.ViolationReportReportedUsers)
                .HasForeignKey(d => d.ReportedUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ViolationReport_reported_user_id_fkey");

            entity.HasOne(d => d.Reporter).WithMany(p => p.ViolationReportReporters)
                .HasForeignKey(d => d.ReporterId)
                .HasConstraintName("ViolationReport_reporter_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
