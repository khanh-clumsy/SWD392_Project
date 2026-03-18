using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Services;

namespace SWD302_Project_HostelManagement.Controllers
{
    /// <summary>
    /// TenantController đóng vai trò TenantInteraction (Boundary) cho:
    ///   - UC7 (Cancel Booking): BookingRequestIndex, CancelConfirm, CancelBooking
    ///   - UC8 (Submit Booking Request): BookRoom (GET/POST)
    ///
    /// LƯU Ý về authentication:
    ///   AuthController lưu TenantId vào ClaimTypes.NameIdentifier khi login.
    ///   Không có "ProfileId" claim — lấy tenantId qua GetCurrentTenantIdAsync().
    /// </summary>
    public class TenantController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CancelCoordinator _cancelCoordinator;
        private readonly BookingCoordinator _bookingCoordinator;

        public TenantController(
            AppDbContext context,
            CancelCoordinator cancelCoordinator,
            BookingCoordinator bookingCoordinator)
        {
            _context = context;
            _cancelCoordinator = cancelCoordinator;
            _bookingCoordinator = bookingCoordinator;
        }

        // ============================================================
        // Helper: lấy TenantId từ ClaimTypes.NameIdentifier
        // (khớp với AuthController — lưu tenant.TenantId vào NameIdentifier)
        // ============================================================
        private async Task<int?> GetCurrentTenantIdAsync()
        {
            var tenantIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(tenantIdStr)) return null;
            if (!int.TryParse(tenantIdStr, out var tenantId)) return null;

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);

            return tenant != null ? tenantId : null;
        }

        // ============================================================
        // UC7 - VIEW BOOKING HISTORY (Extends UC06)
        // GET: /Tenant/BookingRequestIndex
        // TenantInteraction: hiển thị danh sách booking với nút Cancel
        // ============================================================

        /// <summary>
        /// UC7 - Hiển thị danh sách booking của tenant.
        /// Các booking Pending có nút "Cancel Request".
        /// Booking đã Approved/Rejected/Cancelled → nút bị disabled (EX 7.1).
        /// </summary>
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> BookingRequestIndex()
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (tenantId == null)
                return RedirectToAction("Index", "Home");

            var bookings = await _context.BookingRequests
                .Include(b => b.Room)
                    .ThenInclude(r => r.Hostel)
                .Where(b => b.TenantId == tenantId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }

        // ============================================================
        // UC7 - CANCEL BOOKING (M1 → M2 → CancelCoordinator)
        // ============================================================

        /// <summary>
        /// UC7 - GET: /Tenant/CancelConfirm/{id}
        /// TenantInteraction nhận M1 (Cancel Booking Request) từ Tenant.
        /// Hiển thị màn hình xác nhận trước khi gửi M2.
        /// EX 7.1: Nếu status != Pending → redirect với thông báo lỗi.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CancelConfirm(int id)
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (tenantId == null)
                return RedirectToAction("Index", "Home");

            var booking = await _context.BookingRequests
                .Include(b => b.Room)
                    .ThenInclude(r => r.Hostel)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.TenantId == tenantId);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction(nameof(BookingRequestIndex));
            }

            // EX 7.1: Request already processed → cancel button disabled
            if (booking.Status != "Pending")
            {
                TempData["Error"] =
                    $"This booking cannot be cancelled. Current status: {booking.Status}.";
                return RedirectToAction(nameof(BookingRequestIndex));
            }

            return View(booking);
        }

        /// <summary>
        /// UC7 - POST: /Tenant/CancelBooking/{id}
        /// TenantInteraction gửi M2 (Cancel Request) → CancelCoordinator.cancelBooking()
        /// Hiển thị M10 (User Output): CANCEL_SUCCESS / NOT_CANCELLABLE / CANCEL_FAILED
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (tenantId == null)
                return RedirectToAction("Index", "Home");

            // M2: TenantInteraction → CancelCoordinator.cancelBooking(bookingId, tenantId)
            int tenantIdValue = tenantId.Value;
            (bool success, string errorCode) = await _cancelCoordinator.CancelBookingAsync(id, tenantIdValue);

            if (success)
            {
                // M10: display('CANCEL_SUCCESS')
                TempData["Success"] = "Your booking has been successfully cancelled.";
            }
            else if (errorCode == "INVALID_INPUT")
            {
                TempData["Error"] = "Invalid request. Please try again.";
            }
            else if (errorCode is "NOT_FOUND" or "UNAUTHORIZED")
            {
                TempData["Error"] = "Booking not found or you are not authorized to cancel it.";
            }
            else if (errorCode.StartsWith("NOT_CANCELLABLE"))
            {
                // M10: display('NOT_CANCELLABLE', status)
                var status = errorCode.Contains(":")
                    ? errorCode.Split(':', 2)[1] : "";
                TempData["Error"] = string.IsNullOrEmpty(status)
                    ? "This booking cannot be cancelled."
                    : $"This booking cannot be cancelled. Current status: {status}.";
            }
            else
            {
                // M10: display('CANCEL_FAILED')
                TempData["Error"] =
                    "An error occurred while cancelling your booking. Please try again.";
            }

            return RedirectToAction(nameof(BookingRequestIndex));
        }

        // ============================================================
        // UC8 - SUBMIT BOOKING REQUEST
        // GET/POST: /Tenant/BookRoom/{roomId}
        // ============================================================

        /// <summary>
        /// UC8 - GET: /Tenant/BookRoom/{roomId}
        /// TenantInteraction nhận M1 (Submit Booking Request) từ Tenant.
        /// Hiển thị form đặt phòng với thông tin phòng.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> BookRoom(int roomId)
        {
            if (roomId <= 0)
            {
                TempData["Error"] = "Invalid room selection.";
                return RedirectToAction("Index", "Home");
            }

            var room = await _context.Rooms
                .Include(r => r.Hostel)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
            {
                TempData["Error"] = "Room not found.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra trước khi hiển thị form (M4A.2 early check)
            if (room.Status == "Occupied" || room.Status == "Maintenance")
            {
                // M4A.2: displayRoomNotAvailable(roomId)
                TempData["Warning"] =
                    $"Room {room.RoomNumber} is currently not available.";
                return RedirectToAction("Index", "Home");
            }

            return View(room);
        }

        /// <summary>
        /// UC8 - POST: /Tenant/BookRoom/{roomId}
        /// TenantInteraction gửi M2 (Send Booking Data) → BookingCoordinator.submitBookingRequest()
        /// Hiển thị M15 (BOOKING_SUCCESS) / M4A.2 (ROOM_NOT_AVAILABLE) / M6A.1 (INVALID_BOOKING_DATA)
        ///
        /// UC8 pseudocode 5.1:
        ///   tenantId = Session.getCurrentTenantId()   → ClaimTypes.NameIdentifier
        ///   bookingCoordinator.submitBookingRequest(tenantId, roomId, data)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> BookRoom(int roomId, DateOnly checkIn, DateOnly checkOut)
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (tenantId == null)
                return RedirectToAction("Login", "Auth");

            // Validate tại Boundary (UC8 pseudocode 5.1: IF roomId <= 0 OR data IS NULL)
            if (roomId <= 0 || checkIn == default || checkOut == default)
            {
                // M6A.1: displayInvalidBookingData('INVALID_INPUT')
                TempData["Error"] = "Invalid input. Please fill in all required fields.";
                return RedirectToAction(nameof(BookRoom), new { roomId });
            }

            var data = new BookingDTO
            {
                CheckIn = checkIn,
                CheckOut = checkOut
            };

            // M2: TenantInteraction → BookingCoordinator.submitBookingRequest(tenantId, roomId, data)
            int tenantIdValue = tenantId.Value;
            (bool success, string resultCode) =
                await _bookingCoordinator.SubmitBookingRequestAsync(tenantIdValue, roomId, data);

            if (success)
            {
                // M15: displaySuccess('BOOKING_SUCCESS')
                TempData["Success"] =
                    "Your booking request has been submitted successfully! " +
                    "You will receive a confirmation email shortly.";
                return RedirectToAction(nameof(BookingRequestIndex));
            }

            // Xử lý luồng thất bại
            if (resultCode.StartsWith("ROOM_NOT_AVAILABLE"))
            {
                // M4A.2: displayRoomNotAvailable(roomId)
                TempData["Warning"] =
                    "Sorry, this room is no longer available for the selected dates. " +
                    "Please choose different dates.";
            }
            else if (resultCode.StartsWith("INVALID_BOOKING_DATA"))
            {
                // M6A.1: displayInvalidBookingData(errorMsg)
                var detail = resultCode.Contains(":")
                    ? resultCode.Split(':', 2)[1]
                    : "Invalid booking data.";
                TempData["Error"] = $"Booking data is invalid: {detail}";
            }
            else
            {
                TempData["Error"] =
                    "An error occurred while processing your booking. Please try again.";
            }

            return RedirectToAction(nameof(BookRoom), new { roomId });
        }

        // ============================================================
        // CreateBooking (giữ nguyên từ code cũ — KHÔNG xóa)
        // POST: Tenant/CreateBooking
        // ============================================================

        /// <summary>
        /// Tạo booking đơn giản không có checkIn/checkOut (code cũ giữ nguyên).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(int roomId)
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (tenantId == null)
                return RedirectToAction("Index", "Home");

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null || room.Status != "Available")
            {
                TempData["Error"] = "This room is no longer available.";
                return RedirectToAction("Index", "Home");
            }

            var booking = new BookingRequest
            {
                RoomId = roomId,
                TenantId = tenantId.Value,
                RequestType = "Booking",
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _context.BookingRequests.AddAsync(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking request submitted successfully!";
            return RedirectToAction(nameof(BookingRequestIndex));
        }
    }
}
