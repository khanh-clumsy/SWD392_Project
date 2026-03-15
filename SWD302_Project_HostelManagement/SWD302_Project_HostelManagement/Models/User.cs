using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<BookingRequest> BookingRequests { get; set; } = new List<BookingRequest>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Hostel> Hostels { get; set; } = new List<Hostel>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<RoomUpdateLog> RoomUpdateLogs { get; set; } = new List<RoomUpdateLog>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual ICollection<ViolationReport> ViolationReportReportedUsers { get; set; } = new List<ViolationReport>();

    public virtual ICollection<ViolationReport> ViolationReportReporters { get; set; } = new List<ViolationReport>();
}
