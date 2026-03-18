using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Proxies;

namespace SWD302_Project_HostelManagement.Controllers
{
    [Route("BookingRequests")]
    [Authorize(Roles = "HostelOwner")]
    public class BookingRequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailProxy _emailProxy;
        private readonly ILogger<BookingRequestsController> _logger;

        public BookingRequestsController(AppDbContext context, EmailProxy emailProxy, ILogger<BookingRequestsController> logger)
        {
            _context = context;
            _emailProxy = emailProxy;
            _logger = logger;
        }

        // GET: BookingRequests
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            // Get OwnerId from ClaimTypes.NameIdentifier (set during login)
            var ownerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(ownerIdStr, out int ownerId))
            {
                _logger.LogWarning("Invalid or missing OwnerId claim for user");
                return RedirectToAction("Login", "Auth");
            }

            var appDbContext = _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .Where(b => b.Room.OwnerId == ownerId);
            return View(await appDbContext.ToListAsync());
        }

        // GET: BookingRequests/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingRequest = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookingRequest == null)
            {
                return NotFound();
            }

            return View(bookingRequest);
        }

        // POST: BookingRequests/Approve/5
        [HttpPost("Approve/{id}")]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            // if booking.isPending() then
            if (booking != null && booking.Status == "Pending")
            {
                var room = booking.Room;

                // if room.isAvailable() then
                if (room != null && room.Status == "Available")
                {
                    // booking.updateStatus("Approved")
                    booking.UpdateStatus("PendingPayment");

                    room.Status = "Occupied";
                    room.UpdatedAt = DateTime.UtcNow;

                    // tenantId = booking.getTenantId()
                    int tenantId = booking.GetTenantId();

                    // tenant = find Tenant by tenantId
                    var tenant = await _context.Tenants
                        .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                    if (tenant != null)
                    {
                        // email = tenant.getEmail()
                        string email = tenant.GetEmail();

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            // M9: Lưu Notification record
                            // notification = Notification.createRecord(bookingId, email, "Booking Approved")
                            var notification = Notification.CreateRecord(booking.BookingId, email, "Booking Approved");
                            notification.MessageContent = $"Dear {tenant.Name}, your booking request for room {booking.Room.RoomNumber} has been approved.";

                            await _context.Notifications.AddAsync(notification);
                            await _context.SaveChangesAsync();

                            // M10: Gửi email qua EmailProxy
                            // emailProxy.sendEmail(email, notification)
                            bool emailSent = _emailProxy.SendEmail(email, notification);

                            if (emailSent)
                            {
                                notification.Status = "Sent";
                                notification.SentAt = DateTime.UtcNow;
                                _context.Notifications.Update(notification);
                                await _context.SaveChangesAsync();
                                TempData["Success"] = $"Booking #{id} approved successfully and email sent to tenant.";
                            }
                            else
                            {
                                TempData["Warning"] = $"Booking #{id} approved, but email notification failed to send.";
                            }
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // POST: BookingRequests/ApproveWithVerification/5 (LUỒNG M4A: Kiểm tra xác minh tenant)
        [HttpPost("ApproveWithVerification/{id}")]
        public async Task<IActionResult> ApproveBookingWithVerification(int id)
        {
            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking != null && booking.Status == "Pending")
            {
                int tenantId = booking.GetTenantId();
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant != null)
                {
                    string email = tenant.GetEmail();

                    // LUỒNG M4A: Kiểm tra xác minh tenant
                    if (!tenant.CheckVerificationStatus())
                    {
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            // M4A.1: Booking Coordinator -> Notification: Create Notification Record
                            var verificationNotif = Notification.CreateRecord(booking.BookingId, email, "Identity Verification Required");
                            verificationNotif.MessageContent = $"Dear {tenant.Name}, your booking request requires identity verification before approval. Please complete your identity verification.";

                            await _context.Notifications.AddAsync(verificationNotif);
                            await _context.SaveChangesAsync();

                            _emailProxy.SendEmail(email, verificationNotif);
                            TempData["Warning"] = $"Booking #{id} requires tenant identity verification before approval.";
                        }

                        return RedirectToAction("Index");
                    }

                    var room = booking.Room;

                    if (room != null && room.Status == "Available")
                    {
                        booking.UpdateStatus("PendingPayment");
                        room.Status = "Occupied";
                        room.UpdatedAt = DateTime.UtcNow;

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            var notification = Notification.CreateRecord(booking.BookingId, email, "Booking Approved");
                            notification.MessageContent = $"Dear {tenant.Name}, your booking request for room {booking.Room.RoomNumber} has been approved.";

                            await _context.Notifications.AddAsync(notification);
                            await _context.SaveChangesAsync();

                            bool emailSent = _emailProxy.SendEmail(email, notification);

                            if (emailSent)
                            {
                                notification.Status = "Sent";
                                notification.SentAt = DateTime.UtcNow;
                                _context.Notifications.Update(notification);
                                await _context.SaveChangesAsync();
                                TempData["Success"] = $"Booking #{id} approved successfully and email sent to tenant.";
                            }
                            else
                            {
                                TempData["Warning"] = $"Booking #{id} approved, but email notification failed to send.";
                            }
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("Reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only pending bookings can be rejected.";
                return RedirectToAction("Index");
            }

            return View(booking);
        }

        // POST: BookingRequests/Reject/5
        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int id, [Bind("BookingId,RejectReason")] BookingRequest bookingRequest)
        {
            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            // if booking.isPending() then
            if (booking != null && booking.Status == "Pending")
            {
                // booking.updateStatus("Rejected")
                booking.UpdateStatus("Rejected");
                booking.RejectReason = bookingRequest.RejectReason;

                // tenantId = booking.getTenantId()
                int tenantId = booking.GetTenantId();

                // tenant = find Tenant by tenantId
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant != null)
                {
                    // email = tenant.getEmail()
                    string email = tenant.GetEmail();

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        // M1A.6: Lưu Notification record
                        // M1A.7: Notification xác nhận tạo thành công
                        // notification = Notification.createRecord(bookingId, email, "Booking Rejected")
                        var notification = Notification.CreateRecord(booking.BookingId, email, "Booking Rejected");
                        notification.MessageContent = $"Dear {tenant.Name}, your booking request has been rejected. Reason: {bookingRequest.RejectReason}";
                        
                        await _context.Notifications.AddAsync(notification);
                        await _context.SaveChangesAsync();

                        // M1A.8: Gửi email qua EmailProxy
                        // emailProxy.sendEmail(email, notification)
                        bool emailSent = _emailProxy.SendEmail(email, notification);

                        if (emailSent)
                        {
                            notification.Status = "Sent";
                            notification.SentAt = DateTime.UtcNow;
                            _context.Notifications.Update(notification);
                            await _context.SaveChangesAsync();
                            TempData["Success"] = $"Booking #{id} rejected and email sent to tenant.";
                        }
                        else
                        {
                            TempData["Warning"] = $"Booking #{id} rejected, but email notification failed to send.";
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // GET: BookingRequests/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId");
            return View();
        }

        // POST: BookingRequests/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,RoomId,TenantId,RequestType,StartDate,EndDate,Status,RejectReason,CreatedDate,UpdatedDate")] BookingRequest bookingRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookingRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId", bookingRequest.RoomId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId", bookingRequest.TenantId);
            return View(bookingRequest);
        }

        // GET: BookingRequests/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingRequest = await _context.BookingRequests.FindAsync(id);
            if (bookingRequest == null)
            {
                return NotFound();
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId", bookingRequest.RoomId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId", bookingRequest.TenantId);
            return View(bookingRequest);
        }

        // POST: BookingRequests/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,RoomId,TenantId,RequestType,StartDate,EndDate,Status,RejectReason,CreatedDate,UpdatedDate")] BookingRequest bookingRequest)
        {
            if (id != bookingRequest.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookingRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingRequestExists(bookingRequest.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomId", bookingRequest.RoomId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "TenantId", "TenantId", bookingRequest.TenantId);
            return View(bookingRequest);
        }

        // GET: BookingRequests/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingRequest = await _context.BookingRequests
                .Include(b => b.Room)
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookingRequest == null)
            {
                return NotFound();
            }

            return View(bookingRequest);
        }

        // POST: BookingRequests/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookingRequest = await _context.BookingRequests.FindAsync(id);
            if (bookingRequest != null)
            {
                _context.BookingRequests.Remove(bookingRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingRequestExists(int id)
        {
            return _context.BookingRequests.Any(e => e.BookingId == id);
        }
    }
}
