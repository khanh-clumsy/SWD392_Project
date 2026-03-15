using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class RoomUpdateLog
{
    public int LogId { get; set; }

    public int RoomId { get; set; }

    public int? BookingId { get; set; }

    public int? ChangedBy { get; set; }

    public string StatusBefore { get; set; } = null!;

    public string StatusAfter { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public virtual BookingRequest? Booking { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual Room Room { get; set; } = null!;
}
