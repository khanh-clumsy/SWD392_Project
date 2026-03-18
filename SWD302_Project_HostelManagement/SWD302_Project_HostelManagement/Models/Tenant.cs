using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Tenant
{
    public int TenantId { get; set; }

    // ✅ Thông tin đăng nhập — tự lưu, không qua Account
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Status { get; set; } = "Active";
    public string? AvatarUrl { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsVerified { get; set; } = false;

    // ✅ Thông tin profile riêng của Tenant
    public string Name { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? IdentityCard { get; set; }

    // Navigation properties
    public virtual ICollection<BookingRequest> BookingRequests { get; set; }
        = new List<BookingRequest>();
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; }
        = new List<PaymentTransaction>();
    public virtual ICollection<Favorite> Favorites { get; set; }
        = new List<Favorite>();
    public virtual ICollection<Review> Reviews { get; set; }
        = new List<Review>();
    public virtual ICollection<ViolationReport> ViolationReports { get; set; }
        = new List<ViolationReport>();
    public virtual ICollection<ViolationReport> ReportedViolations { get; set; }
        = new List<ViolationReport>();

    /// <summary>
    /// Gets the email address of this tenant
    /// </summary>
    /// <returns>The email address</returns>
    public string GetEmail()
    {
        return Email;
    }

    /// <summary>
    /// Checks the verification status of this tenant
    /// </summary>
    /// <returns>True if tenant is verified, false otherwise</returns>
    public bool CheckVerificationStatus()
    {
        return IsVerified;
    }
}
