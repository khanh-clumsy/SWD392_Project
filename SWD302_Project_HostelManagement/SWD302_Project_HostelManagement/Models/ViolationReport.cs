using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class ViolationReport
{
    public int ReportId { get; set; }

    public int ReporterId { get; set; }

    public int? ReportedUserId { get; set; }

    public int? HostelId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Evidence { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public virtual Hostel? Hostel { get; set; }

    public virtual User? ReportedUser { get; set; }

    public virtual User Reporter { get; set; } = null!;
}
