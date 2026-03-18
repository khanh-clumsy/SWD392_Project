using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Proxies;

namespace SWD302_Project_HostelManagement.Services
{
    
    public class BookingCoordinator
    {
        private readonly AppDbContext _context;
        private readonly EmailProxy _emailProxy;
        private readonly ILogger<BookingCoordinator> _logger;

        public BookingCoordinator(
            AppDbContext context,
            EmailProxy emailProxy,
            ILogger<BookingCoordinator> logger)
        {
            _context = context;
            _emailProxy = emailProxy;
            _logger = logger;
        }

        
        public async Task<(bool Success, string ResultCode)> SubmitBookingRequestAsync(
            int tenantId, int roomId, BookingDTO data)
        {
            // IF tenantId <= 0 OR roomId <= 0 OR data IS NULL
            if (tenantId <= 0 || roomId <= 0 || data == null)
                return (false, HandleInvalidBookingData("INVALID_INPUT"));

            // M3: Room.checkAvailability(roomId, data.checkIn, data.checkOut)
            var availability = await CheckAvailabilityAsync(roomId, data.CheckIn, data.CheckOut);

            // M4A[Occupied]: handleRoomUnavailable(roomId)
            if (availability == AvailabilityStatus.Occupied ||
                availability == AvailabilityStatus.NotFound)
                return (false, HandleRoomUnavailable(roomId));

            // M5: BookingRequest.createBooking(tenantId, roomId, data)
            BookingRequest booking;
            try
            {
                booking = await CreateBookingAsync(tenantId, roomId, data);
            }
            catch (InvalidBookingDataException ex)
            {
                // M6A[Invalid]: handleInvalidBookingData
                return (false, HandleInvalidBookingData(ex.Message));
            }

            // M9: sendEmailNotification(booking.bookingId, tenantId)
            await SendEmailNotificationAsync(booking.BookingId, tenantId);

            // M15: TenantInteraction.display('BOOKING_SUCCESS')
            return (true, "BOOKING_SUCCESS");
        }

      
        private async Task<AvailabilityStatus> CheckAvailabilityAsync(
            int roomId, DateOnly checkIn, DateOnly checkOut)
        {
            if (roomId <= 0)
                return AvailabilityStatus.Occupied;

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return AvailabilityStatus.NotFound;

            // IF room.status = 'MAINTENANCE' RETURN OCCUPIED
            if (room.Status == "Maintenance")
                return AvailabilityStatus.Occupied;

            // IF room.status = 'Occupied' luôn từ chối
            if (room.Status == "Occupied")
                return AvailabilityStatus.Occupied;

            // conflicts = DB.countBookings(statusIn: ['Pending','Approved'], overlaps)
            int conflicts = await _context.BookingRequests
                .Where(b => b.RoomId == roomId
                    && (b.Status == "Pending" || b.Status == "Approved")
                    && b.StartDate != null && b.EndDate != null
                    && b.StartDate < checkOut
                    && b.EndDate > checkIn)
                .CountAsync();

            return conflicts == 0
                ? AvailabilityStatus.Available   // M4[Available]
                : AvailabilityStatus.Occupied;   // M4A[Occupied]
        }


        private async Task<BookingRequest> CreateBookingAsync(
            int tenantId, int roomId, BookingDTO data)
        {
            // IF data.checkOut <= data.checkIn THROW InvalidBookingDataException
            if (data.CheckOut <= data.CheckIn)
                throw new InvalidBookingDataException("check-out must be after check-in");

            var record = new BookingRequest
            {
                TenantId = tenantId,
                RoomId = roomId,
                StartDate = data.CheckIn,
                EndDate = data.CheckOut,
                RequestType = "Booking",
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _context.BookingRequests.AddAsync(record);
            await _context.SaveChangesAsync();
            return record;
        }

    
        private async Task SendEmailNotificationAsync(int bookingId, int tenantId)
        {
            // IF bookingId <= 0 OR tenantId <= 0 THEN RETURN
            if (bookingId <= 0 || tenantId <= 0) return;

            // M7: email = Tenant.findById(tenantId).email
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);

            string? email = tenant?.Email;
            if (string.IsNullOrWhiteSpace(email)) return;

            // M7: Notification.createPendingNotification(bookingId, email)
            var notif = new Notification
            {
                BookingId = bookingId,
                RecipientEmail = email,
                Subject = $"Booking Confirmation - Booking #{bookingId}",
                MessageContent =
                    $"Dear {tenant?.Name}, " +
                    $"your booking request (ID: #{bookingId}) has been submitted successfully " +
                    $"and is pending approval from the hostel owner.",
                Type = "BookingConfirm",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notif);
            await _context.SaveChangesAsync();

            // M9→M10: EmailServiceProxy.send(email, notif.content)
            bool delivered = _emailProxy.SendEmail(email, notif);

            // M13: Notification.updateNotificationStatus(notif.id, 'SENT'/'FAILED')
            notif.Status = delivered ? "Sent" : "Failed";
            if (delivered) notif.SentAt = DateTime.UtcNow;

            _context.Notifications.Update(notif);
            await _context.SaveChangesAsync();

            if (!delivered)
                _logger.LogWarning(
                    "sendEmailNotification: gửi email thất bại cho booking {Id}", bookingId);
        }

        
        private string HandleRoomUnavailable(int roomId)
            => $"ROOM_NOT_AVAILABLE:{roomId}";

        
        private string HandleInvalidBookingData(string errorMsg)
            => $"INVALID_BOOKING_DATA:{errorMsg}";
    }

    // DTO truyền dữ liệu đặt phòng từ TenantInteraction → BookingCoordinator (UC8)
    public class BookingDTO
    {
        public DateOnly CheckIn { get; set; }
        public DateOnly CheckOut { get; set; }
    }

    // Enum AvailabilityStatus theo UC8 diagram (M4 / M4A)
    public enum AvailabilityStatus
    {
        Available,
        Occupied,
        NotFound
    }

    // Exception theo UC8 pseudocode 3.5
    public class InvalidBookingDataException : Exception
    {
        public InvalidBookingDataException(string message) : base(message) { }
    }
}
