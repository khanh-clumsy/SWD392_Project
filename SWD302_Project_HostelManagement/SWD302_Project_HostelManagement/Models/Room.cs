using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int HostelId { get; set; }

    public int OwnerId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string? RoomType { get; set; }

    public int? Capacity { get; set; }

    public decimal PricePerMonth { get; set; }

    public decimal? Area { get; set; }

    public int? Floor { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<BookingRequest> BookingRequests { get; set; } = new List<BookingRequest>();

    public virtual Hostel Hostel { get; set; } = null!;

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<RoomUpdateLog> RoomUpdateLogs { get; set; } = new List<RoomUpdateLog>();
}
