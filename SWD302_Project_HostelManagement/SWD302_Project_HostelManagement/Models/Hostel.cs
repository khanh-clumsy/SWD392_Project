using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Hostel
{
    public int HostelId { get; set; }

    public int OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = "PendingApproval";

    public string? RejectReason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public virtual HostelOwner Owner { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
