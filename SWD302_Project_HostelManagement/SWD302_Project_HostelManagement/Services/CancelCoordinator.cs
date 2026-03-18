using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Proxies;

namespace SWD302_Project_HostelManagement.Services
{
    /// <summary>
    /// UC7 - CancelCoordinator (Control / Use-case Controller)
    ///
    /// Theo tài liệu UC7:
    ///   cancelBooking()       ← M2 (Cancel Request) từ TenantInteraction
    ///   handleCancelSuccess() ← M6 (Updated Success) từ BookingRequest
    ///                           → gửi M7 EmailServiceProxy.sendCancelNotice
    ///                           → trả M9 CANCEL_SUCCESS về View
    /// </summary>
    public class CancelCoordinator
    {
        private readonly AppDbContext _context;
        private readonly EmailProxy _emailProxy;
        private readonly ILogger<CancelCoordinator> _logger;

        public CancelCoordinator(
            AppDbContext context,
            EmailProxy emailProxy,
            ILogger<CancelCoordinator> logger)
        {
            _context = context;
            _emailProxy = emailProxy;
            _logger = logger;
        }

        /// <summary>
        /// UC7 - 1.1 cancelBooking(bookingId, tenantId)
        ///
        /// Pseudocode:
        ///   IF bookingId <= 0 OR tenantId <= 0 → display('INVALID_INPUT')
        ///   status = BookingRequest.checkStatus(bookingId)
        ///   IF status != 'PENDING' → display('NOT_CANCELLABLE', status)
        ///   success = BookingRequest.cancelBookingRecord(bookingId)
        ///   IF NOT success → display('CANCEL_FAILED')
        ///   handleCancelSuccess(bookingId, tenantId)
        /// </summary>
        public async Task<(bool Success, string ErrorCode)> CancelBookingAsync(int bookingId, int tenantId)
        {
            // IF bookingId <= 0 OR tenantId <= 0
            if (bookingId <= 0 || tenantId <= 0)
                return (false, "INVALID_INPUT");

            // M3: BookingRequest.checkStatus(bookingId)
            var booking = await _context.BookingRequests
                .Include(b => b.Tenant)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return (false, "NOT_FOUND");

            // Chỉ tenant sở hữu booking mới được hủy
            if (booking.TenantId != tenantId)
                return (false, "UNAUTHORIZED");

            // M3: checkStatus → IF status != 'PENDING'
            string status = CheckStatus(booking);
            if (status != "Pending")
                return (false, $"NOT_CANCELLABLE:{status}");

            // M5: BookingRequest.cancelBookingRecord(bookingId)
            bool cancelled = await CancelBookingRecordAsync(booking);
            if (!cancelled)
                return (false, "CANCEL_FAILED");

            // M6: Updated Success → handleCancelSuccess(bookingId, tenantId)
            await HandleCancelSuccessAsync(booking);

            // M9: CANCEL_SUCCESS
            return (true, "CANCEL_SUCCESS");
        }

        /// <summary>
        /// UC7 - BookingRequest.checkStatus(bookingId)
        /// M3: Trả về status hiện tại của booking
        /// </summary>
        private string CheckStatus(BookingRequest booking)
        {
            return booking.Status;
        }

        /// <summary>
        /// UC7 - BookingRequest.cancelBookingRecord(bookingId)
        /// M5: record.status = 'CANCELLED'; record.cancelledAt = NOW()
        /// Pseudocode: IF record.status != 'PENDING' THROW BusinessRuleException
        /// </summary>
        private async Task<bool> CancelBookingRecordAsync(BookingRequest booking)
        {
            try
            {
                if (booking.Status != "Pending")
                {
                    _logger.LogWarning(
                        "cancelBookingRecord: booking {Id} không ở trạng thái Pending (status={Status})",
                        booking.BookingId, booking.Status);
                    return false;
                }

                // record.status = 'CANCELLED'; record.cancelledAt = NOW()
                booking.Status = "Cancelled";
                booking.CancelledAt = DateTime.UtcNow;
                booking.UpdatedDate = DateTime.UtcNow;

                _context.BookingRequests.Update(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "cancelBookingRecord thất bại cho booking {Id}", booking.BookingId);
                return false;
            }
        }

        /// <summary>
        /// UC7 - 1.2 handleCancelSuccess(bookingId, tenantId)
        ///
        /// Pseudocode:
        ///   EmailServiceProxy.sendCancelNotice(bookingId, tenantId)
        ///   TenantInteraction.display('CANCEL_SUCCESS')
        ///
        /// LƯU Ý: Tenant.Email lấy trực tiếp (không qua Account) vì
        /// project dùng single-table per role, Tenant.Email là field riêng.
        /// </summary>
        private async Task HandleCancelSuccessAsync(BookingRequest booking)
        {
            // M7: Lấy email của tenant — lấy trực tiếp từ Tenant.Email
            string? email = booking.Tenant?.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning(
                    "handleCancelSuccess: không tìm thấy email cho tenantId={Id}",
                    booking.TenantId);
                return;
            }

            // M7: Tạo Notification record với status = 'Pending'
            var notification = new Notification
            {
                BookingId = booking.BookingId,
                RecipientEmail = email,
                Subject = $"Booking Cancelled - Room {booking.Room?.RoomNumber}",
                MessageContent =
                    $"Dear {booking.Tenant?.Name}, " +
                    $"your booking request (ID: #{booking.BookingId}) " +
                    $"for room {booking.Room?.RoomNumber} has been successfully cancelled.",
                Type = "BookingCancelled",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // M7→M8: EmailServiceProxy.sendCancelNotice → Email Delivery Service
            bool sent = _emailProxy.SendEmail(email, notification);

            // M13: Cập nhật notification status (SENT / FAILED)
            notification.Status = sent ? "Sent" : "Failed";
            if (sent) notification.SentAt = DateTime.UtcNow;

            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            if (!sent)
                _logger.LogWarning(
                    "handleCancelSuccess: gửi email thất bại cho booking {Id}", booking.BookingId);
        }
    }
}
