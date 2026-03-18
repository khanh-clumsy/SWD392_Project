using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class BookingRequest
{
    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public int TenantId { get; set; }

    public string RequestType { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; }

    public string? RejectReason { get; set; }

    /// <summary>
    /// UC7 DB design: cancelled_at TIMESTAMP NULLABLE
    /// Set khi CancelCoordinator.cancelBookingRecord() thực thi thành công:
    ///   record.cancelledAt = NOW()
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual Room Room { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<RoomUpdateLog> RoomUpdateLogs { get; set; } = new List<RoomUpdateLog>();

    /// <summary>
    /// Updates the booking status
    /// </summary>
    public void UpdateStatus(string newStatus)
    {
        Status = newStatus;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the room ID for this booking
    /// </summary>
    public int GetRoomId()
    {
        return RoomId;
    }

    /// <summary>
    /// Gets the tenant ID for this booking
    /// </summary>
    public int GetTenantId()
    {
        return TenantId;
    }

    /// <summary>
    /// Checks if booking is in pending status
    /// </summary>
    public bool IsPending()
    {
        return Status == "Pending";
    }
}
