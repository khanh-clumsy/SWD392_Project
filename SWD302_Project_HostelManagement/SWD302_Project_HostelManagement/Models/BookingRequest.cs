using System;
using System.Collections.Generic;
using SWD302_Project_HostelManagement.Data;

namespace SWD302_Project_HostelManagement.Models;

public partial class BookingRequest
{
    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public int TenantId { get; set; }

    public string RequestType { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal DepositAmount { get; set; }

    public string Status { get; set; }

    public string? RejectReason { get; set; }

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
    /// <param name="newStatus">The new status value</param>
    public void UpdateStatus(string newStatus)
    {
        Status = newStatus;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the room ID for this booking
    /// </summary>
    /// <returns>The room ID</returns>
    public int GetRoomId()
    {
        return RoomId;
    }

    /// <summary>
    /// Gets the tenant ID for this booking
    /// </summary>
    /// <returns>The tenant ID</returns>
    public int GetTenantId()
    {
        return TenantId;
    }

    /// <summary>
    /// Checks if booking is in pending status
    /// </summary>
    /// <returns>True if status is "Pending", false otherwise</returns>
    public bool IsPending()
    {
        return Status == "Pending";
    }  
}
