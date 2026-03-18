using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Controllers
{
    public class TenantController : Controller
    {
        private readonly AppDbContext _context;

        public TenantController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Helper: Get TenantId from ClaimsPrincipal
        /// Reads TenantId from ClaimTypes.NameIdentifier
        /// </summary>
        private async Task<int?> GetCurrentTenantIdAsync()
        {
            var tenantIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(tenantIdStr)) return null;

            if (!int.TryParse(tenantIdStr, out var tenantId)) return null;

            // Verify tenant exists
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);

            return tenant != null ? tenantId : null;
        }

        /// <summary>
        /// GET: Tenant/BookingRequestIndex
        /// Display list of bookings for the current logged-in Tenant
        /// Only Tenant role can access
        /// If Status = "Pending Payment" → show "Pay Now" button
        /// </summary>
        public async Task<IActionResult> BookingRequestIndex()
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (tenantId == null)
                return RedirectToAction("Index", "Home");

            // Only get bookings for the current logged-in Tenant
            var bookings = await _context.BookingRequests
                .Include(b => b.Room)
                    .ThenInclude(r => r.Hostel)
                .Where(b => b.TenantId == tenantId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }

        /// <summary>
        /// POST: Tenant/CreateBooking
        /// Create a new BookingRequest with Status = "Pending Payment"
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(int roomId)
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (tenantId == null)
                return RedirectToAction("Index", "Home");

            // Check if Room is still Available
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null || room.Status != "Available")
            {
                TempData["Error"] = "This room is no longer available.";
                return RedirectToAction("Index", "Home");
            }

            // Create BookingRequest with Status = "PendingPayment"
            var booking = new BookingRequest
            {
                RoomId = roomId,
                TenantId = tenantId.Value,
                RequestType = "Booking",
                DepositAmount = room.PricePerMonth,
                Status = "PendingPayment",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.BookingRequests.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking created. Please proceed to payment.";
            return RedirectToAction("BookingRequestIndex");
        }
    }
}
