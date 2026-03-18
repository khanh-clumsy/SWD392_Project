using System;

namespace SWD302_Project_HostelManagement.ViewModels
{
    public class SystemStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalHostels { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBookings { get; set; }
        public int PendingHostelRequests { get; set; }
        public int PendingBookingRequests { get; set; }

        public string TimeFilter { get; set; } = "All"; // All, Today, Last7Days, Last30Days

        // Growth data or recent activity could be added here later
    }
}
