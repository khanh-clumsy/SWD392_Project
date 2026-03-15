using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? BookingId { get; set; }

    public string RecipientEmail { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string MessageContent { get; set; } = null!;

    public string? Type { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual BookingRequest? Booking { get; set; }
}
