using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int TenantId { get; set; }

    public int HostelId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Hostel Hostel { get; set; } = null!;

    public virtual User Tenant { get; set; } = null!;
}
