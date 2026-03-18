using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Proxies;
using SWD302_Project_HostelManagement.ViewModels;
using System.Security.Claims;

namespace SWD302_Project_HostelManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailProxy _emailProxy;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, EmailProxy emailProxy, ILogger<AdminController> logger)
        {
            _context = context;
            _emailProxy = emailProxy;
            _logger = logger;
        }

        // UC19: View System Statistics
        public async Task<IActionResult> Dashboard(string timeFilter = "All")
        {
            DateTime? filterDate = null;
            if (timeFilter == "Today") filterDate = DateTime.UtcNow.Date;
            else if (timeFilter == "Last7Days") filterDate = DateTime.UtcNow.AddDays(-7);
            else if (timeFilter == "Last30Days") filterDate = DateTime.UtcNow.AddDays(-30);

            var stats = new SystemStatisticsViewModel
            {
                TimeFilter = timeFilter
            };

            // Count Users (Admins + Owners + Tenants)
            var adminsQuery = _context.Admins.AsQueryable();
            var ownersQuery = _context.HostelOwners.AsQueryable();
            var tenantsQuery = _context.Tenants.AsQueryable();

            if (filterDate.HasValue)
            {
                adminsQuery = adminsQuery.Where(a => a.CreatedDate >= filterDate.Value);
                ownersQuery = ownersQuery.Where(o => o.CreatedDate >= filterDate.Value);
                tenantsQuery = tenantsQuery.Where(t => t.CreatedDate >= filterDate.Value);
            }

            stats.TotalUsers = await adminsQuery.CountAsync() + await ownersQuery.CountAsync() + await tenantsQuery.CountAsync();

            // Count Hostels
            var hostelsQuery = _context.Hostels.AsQueryable();
            if (filterDate.HasValue) hostelsQuery = hostelsQuery.Where(h => h.CreatedDate >= filterDate.Value);
            stats.TotalHostels = await hostelsQuery.CountAsync();
            stats.PendingHostelRequests = await hostelsQuery.CountAsync(h => h.Status == "PendingApproval");

            // Count Rooms
            var roomsQuery = _context.Rooms.AsQueryable();
            if (filterDate.HasValue) roomsQuery = roomsQuery.Where(r => r.CreatedAt >= filterDate.Value);
            stats.TotalRooms = await roomsQuery.CountAsync();

            // Count Booking Requests
            var bookingsQuery = _context.BookingRequests.AsQueryable();
            if (filterDate.HasValue) bookingsQuery = bookingsQuery.Where(b => b.CreatedDate >= filterDate.Value);
            stats.TotalBookings = await bookingsQuery.CountAsync();
            stats.PendingBookingRequests = await bookingsQuery.CountAsync(b => b.Status == "Pending");

            return View(stats);
        }

        // UC33: View New Hostels Approval List
        public async Task<IActionResult> Index()
        {
            var pendingHostels = await _context.Hostels
                .Include(h => h.Owner)
                .Where(h => h.Status == "PendingApproval")
                .OrderByDescending(h => h.CreatedDate)
                .ToListAsync();

            return View(pendingHostels);
        }

        // UC18: Step 4 - Display detailed hostel information
        public async Task<IActionResult> Details(int id)
        {
            var hostel = await _context.Hostels
                .Include(h => h.Owner)
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HostelId == id);

            if (hostel == null)
            {
                return NotFound();
            }

            if (hostel.Status != "PendingApproval")
            {
                TempData["Error"] = "This hostel request has already been processed.";
                return RedirectToAction(nameof(Index));
            }

            return View(hostel);
        }

        // UC18: Step 6 - Approve Hostel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var hostel = await _context.Hostels
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(h => h.HostelId == id);

            if (hostel == null) return NotFound();

            if (hostel.Status != "PendingApproval")
            {
                TempData["Error"] = "This hostel request has already been processed.";
                return RedirectToAction(nameof(Index));
            }

            // Update status
            hostel.Status = "Approved";
            hostel.UpdatedDate = DateTime.UtcNow;
            _context.Hostels.Update(hostel);

            // Create notification record
            var notification = new Notification
            {
                RecipientEmail = hostel.Owner.Email,
                Subject = "Hostel Registration Approved",
                MessageContent = $"Chúc mừng! Yêu cầu đăng ký hostel \"{hostel.Name}\" của bạn đã được phê duyệt và hiện đã hiển thị trên hệ thống.",
                Type = "HostelApproval",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Send email
            bool emailSent = _emailProxy.SendEmail(hostel.Owner.Email, notification);
            if (emailSent)
            {
                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Hostel \"{hostel.Name}\" đã được phê duyệt thành công.";
            return RedirectToAction(nameof(Index));
        }

        // UC18: Alternative Sequence - Reject Hostel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string rejectReason)
        {
            if (string.IsNullOrWhiteSpace(rejectReason))
            {
                ModelState.AddModelError("RejectReason", "Vui lòng nhập lý do từ chối.");
                var hostelDetails = await _context.Hostels
                    .Include(h => h.Owner)
                    .Include(h => h.Rooms)
                    .FirstOrDefaultAsync(h => h.HostelId == id);
                return View("Details", hostelDetails);
            }

            var hostel = await _context.Hostels
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(h => h.HostelId == id);

            if (hostel == null) return NotFound();

            if (hostel.Status != "PendingApproval")
            {
                TempData["Error"] = "This hostel request has already been processed.";
                return RedirectToAction(nameof(Index));
            }

            // Update status and reason
            hostel.Status = "Rejected";
            hostel.RejectReason = rejectReason;
            hostel.UpdatedDate = DateTime.UtcNow;
            _context.Hostels.Update(hostel);

            // Create notification record
            var notification = new Notification
            {
                RecipientEmail = hostel.Owner.Email,
                Subject = "Hostel Registration Rejected",
                MessageContent = $"Rất tiếc, yêu cầu đăng ký hostel \"{hostel.Name}\" của bạn đã bị từ chối. Lý do: {rejectReason}",
                Type = "HostelRejection",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Send email
            bool emailSent = _emailProxy.SendEmail(hostel.Owner.Email, notification);
            if (emailSent)
            {
                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Hostel \"{hostel.Name}\" đã bị từ chối.";
            return RedirectToAction(nameof(Index));
        }
    }
}
