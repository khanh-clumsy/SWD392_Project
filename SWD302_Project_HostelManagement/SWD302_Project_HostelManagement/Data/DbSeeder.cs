using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Tenants.AnyAsync()) return;

        // =============================================
        // 1. ADMIN
        // =============================================
        var admin = new Admin
        {
            Email = "admin@hostel.com",
            PasswordHash = "admin123456",
            Name = "Super Admin",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        await context.Admins.AddAsync(admin);
        await context.SaveChangesAsync();

        // =============================================
        // 2. HOSTEL OWNERS
        // =============================================
        var owners = new List<HostelOwner>
        {
            new HostelOwner
            {
                Email    = "owner1@hostel.com",
                PasswordHash = "owner1234567",
                Name     = "Nguyễn Văn An",
                PhoneNumber = "0901234567",
                BusinessLicense = "BL-2024-001",
                Status   = "Active",
                CreatedDate = DateTime.UtcNow
            },
            new HostelOwner
            {
                Email    = "owner2@hostel.com",
                PasswordHash = "owner2234567",
                Name     = "Trần Thị Bình",
                PhoneNumber = "0912345678",
                BusinessLicense = "BL-2024-002",
                Status   = "Active",
                CreatedDate = DateTime.UtcNow
            }
        };
        await context.HostelOwners.AddRangeAsync(owners);
        await context.SaveChangesAsync();

        // =============================================
        // 3. TENANTS
        // Mỗi tenant dùng để test 1 nhóm trường hợp
        // =============================================
        var tenants = new List<Tenant>
        {
            // tenant1 – test UC7: có booking Pending để Cancel
            new Tenant
            {
                Email    = "tenant1@gmail.com",
                PasswordHash = "tenant1234567",
                Name     = "Lê Văn Cường",
                PhoneNumber = "0923456789",
                IdentityCard = "079200012345",
                Status   = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // tenant2 – test UC8: đặt phòng mới (Available room)
            new Tenant
            {
                Email    = "tenant2@gmail.com",
                PasswordHash = "tenant2234567",
                Name     = "Phạm Thị Dung",
                PhoneNumber = "0934567890",
                IdentityCard = "079200023456",
                Status   = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // tenant3 – test UC7 EX 7.1: có booking đã Approved/Rejected (không thể cancel)
            new Tenant
            {
                Email    = "tenant3@gmail.com",
                PasswordHash = "tenant3234567",
                Name     = "Hoàng Văn Em",
                PhoneNumber = "0945678901",
                IdentityCard = "079200034567",
                Status   = "Active",
                CreatedDate = DateTime.UtcNow
            },
            // tenant4 – test UC8 AS 8.2: cố đặt phòng Occupied
            new Tenant
            {
                Email    = "tenant4@gmail.com",
                PasswordHash = "tenant4234567",
                Name     = "Vũ Thị Hoa",
                PhoneNumber = "0956789012",
                IdentityCard = "079200045678",
                Status   = "Active",
                CreatedDate = DateTime.UtcNow
            }
        };
        await context.Tenants.AddRangeAsync(tenants);
        await context.SaveChangesAsync();

        // =============================================
        // 4. HOSTELS
        // =============================================
        var hostels = new List<Hostel>
        {
            new Hostel
            {
                new Hostel
                {
                    OwnerId = owners[0].OwnerId,
                    Name = "Cozy Hostel Downtown",
                    Address = "123 Nguyen Hue, District 1, HCMC",
                    Price = 1950000,
                    Description = "Budget-friendly hostel in the heart of downtown",
                    Status = "Approved",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Hostel
                {
                    OwnerId = owners[1].OwnerId,
                    Name = "Modern City Hostel",
                    Address = "456 Tran Hung Dao, District 5, HCMC",
                    Price = 1750000,
                    Description = "Modern facilities with shared kitchens",
                    Status = "Approved",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };
            await context.Hostels.AddRangeAsync(hostels);
            await context.SaveChangesAsync();

        // =============================================
        // 5. ROOMS – đủ trạng thái để test UC8
        // =============================================
        var rooms = new List<Room>
        {
            // [0] Hostel 1 – Occupied: test UC7 (tenant1 đang ở), UC8 AS 8.2 (phòng không available)
            new Room
            {
                HostelId = hostels[0].HostelId,
                OwnerId  = owners[0].OwnerId,
                RoomNumber = "101",
                RoomType = "Single",
                Capacity = 1,
                PricePerMonth = 2500000,
                Area = 20,
                Floor = 1,
                Status = "Occupied",
                Description = "Phòng đơn, có máy lạnh, WC riêng",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // [1] Hostel 1 – Available: test UC8 happy path (tenant2 đặt)
            new Room
            {
                HostelId = hostels[0].HostelId,
                OwnerId  = owners[0].OwnerId,
                RoomNumber = "102",
                RoomType = "Double",
                Capacity = 2,
                PricePerMonth = 3500000,
                Area = 25,
                Floor = 1,
                Status = "Available",
                Description = "Phòng đôi, ban công, view đường, nội thất đầy đủ",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // [2] Hostel 1 – Available: test UC8 conflict (đã có Pending booking trùng ngày)
            new Room
            {
                HostelId = hostels[0].HostelId,
                OwnerId  = owners[0].OwnerId,
                RoomNumber = "103",
                RoomType = "Single",
                Capacity = 1,
                PricePerMonth = 2800000,
                Area = 18,
                Floor = 2,
                Status = "Available",
                Description = "Phòng đơn tầng 2, yên tĩnh, có cửa sổ lớn",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // [3] Hostel 3 – Available: test UC8 Viewing request
            new Room
            {
                HostelId = hostels[2].HostelId,
                OwnerId  = owners[1].OwnerId,
                RoomNumber = "A01",
                RoomType = "Studio",
                Capacity = 2,
                PricePerMonth = 5000000,
                Area = 35,
                Floor = 3,
                Status = "Available",
                Description = "Studio full nội thất, bếp riêng, thang máy",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // [4] Hostel 3 – Occupied: test UC8 AS 8.2 (room không available)
            new Room
            {
                HostelId = hostels[2].HostelId,
                OwnerId  = owners[1].OwnerId,
                RoomNumber = "A02",
                RoomType = "Studio",
                Capacity = 2,
                PricePerMonth = 5500000,
                Area = 40,
                Floor = 4,
                Status = "Occupied",
                Description = "Studio cao cấp, view thành phố, nội thất nhập khẩu",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // [5] Hostel 1 – Maintenance: test UC8 AS 8.2 (room bảo trì)
            new Room
            {
                HostelId = hostels[0].HostelId,
                OwnerId  = owners[0].OwnerId,
                RoomNumber = "104",
                RoomType = "Double",
                Capacity = 2,
                PricePerMonth = 3200000,
                Area = 22,
                Floor = 3,
                Status = "Maintenance",
                Description = "Đang bảo trì, sơn mới, dự kiến xong 1 tuần",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();

        // =============================================
        // 6. BOOKING REQUESTS – đủ status để test UC7
        // =============================================
        var bookings = new List<BookingRequest>
        {
            // [0] UC7 happy path: tenant1 – Pending → có thể Cancel
            new BookingRequest
            {
                RoomId  = rooms[0].RoomId,
                TenantId = tenants[0].TenantId,
                RequestType = "Booking",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                EndDate   = DateOnly.FromDateTime(DateTime.Today.AddDays(35)),
                Status    = "Pending",
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                UpdatedDate = DateTime.UtcNow.AddDays(-2)
            },
            // [1] UC7 EX 7.1: tenant3 – Approved → nút Cancel disabled
            new BookingRequest
            {
                RoomId  = rooms[0].RoomId,
                TenantId = tenants[2].TenantId,
                RequestType = "Booking",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-60)),
                EndDate   = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
                Status    = "Approved",
                CreatedDate = DateTime.UtcNow.AddDays(-65),
                UpdatedDate = DateTime.UtcNow.AddDays(-60)
            },
            // [2] UC7 EX 7.1: tenant3 – Rejected → nút Cancel disabled
            new BookingRequest
            {
                RoomId  = rooms[2].RoomId,
                TenantId = tenants[2].TenantId,
                RequestType = "Viewing",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                EndDate   = null,
                Status    = "Rejected",
                RejectReason = "Phòng đã có người đặt trước trong thời gian này",
                CreatedDate = DateTime.UtcNow.AddDays(-12),
                UpdatedDate = DateTime.UtcNow.AddDays(-10)
            },
            // [3] UC7: tenant1 – Cancelled (đã hủy trước đó, có CancelledAt)
            new BookingRequest
            {
                RoomId  = rooms[1].RoomId,
                TenantId = tenants[0].TenantId,
                RequestType = "Booking",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-20)),
                EndDate   = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
                Status    = "Cancelled",
                CancelledAt = DateTime.UtcNow.AddDays(-18),
                CreatedDate = DateTime.UtcNow.AddDays(-22),
                UpdatedDate = DateTime.UtcNow.AddDays(-18)
            },
            // [4] UC8 conflict test: booking Pending trùng ngày với rooms[2]
            // Khi tenant2 cố đặt rooms[2] cùng period → AS 8.2 (conflict)
            new BookingRequest
            {
                RoomId  = rooms[2].RoomId,
                TenantId = tenants[0].TenantId,
                RequestType = "Booking",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate   = DateOnly.FromDateTime(DateTime.Today.AddDays(60)),
                Status    = "Pending",
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                UpdatedDate = DateTime.UtcNow.AddDays(-1)
            },
            // [5] DepositPaid: tenant1 – test Pay Now button
            new BookingRequest
            {
                RoomId  = rooms[3].RoomId,
                TenantId = tenants[0].TenantId,
                RequestType = "Booking",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                EndDate   = DateOnly.FromDateTime(DateTime.Today.AddDays(40)),
                Status    = "DepositPaid",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                UpdatedDate = DateTime.UtcNow.AddDays(-10)
            },
            // [6] tenant2 – Pending (để test UC7: tenant2 cancel booking của mình)
            new BookingRequest
            {
                RoomId  = rooms[3].RoomId,
                TenantId = tenants[1].TenantId,
                RequestType = "Booking",
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                EndDate   = DateOnly.FromDateTime(DateTime.Today.AddDays(33)),
                Status    = "Pending",
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                UpdatedDate = DateTime.UtcNow.AddDays(-1)
            }
        };
        await context.BookingRequests.AddRangeAsync(bookings);
        await context.SaveChangesAsync();

        // =============================================
        // 7. PAYMENT TRANSACTIONS
        // =============================================
        var payments = new List<PaymentTransaction>
        {
            new PaymentTransaction
            {
                BookingId = bookings[5].BookingId,
                TenantId  = tenants[0].TenantId,
                Amount    = 5000000,
                PaymentMethod = "VNPay",
                GatewayRef = "PAY-2026-001",
                Status    = "Success",
                PaidAt    = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };
        await context.PaymentTransactions.AddRangeAsync(payments);
        await context.SaveChangesAsync();

        // =============================================
        // 8. NOTIFICATIONS
        // =============================================
        var notifications = new List<Notification>
        {
            // Booking Approved notification
            new Notification
            {
                BookingId = bookings[1].BookingId,
                RecipientEmail = tenants[2].Email,
                Subject = "Booking Approved - Nhà Trọ Ánh Dương Room 101",
                MessageContent = "Dear Hoàng Văn Em, your booking request for room 101 has been approved.",
                Type    = "BookingApproved",
                Status  = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                SentAt  = DateTime.UtcNow.AddDays(-60)
            },
            // Booking Rejected notification
            new Notification
            {
                BookingId = bookings[2].BookingId,
                RecipientEmail = tenants[2].Email,
                Subject = "Booking Rejected - Phòng 103",
                MessageContent = "Dear Hoàng Văn Em, your viewing request has been rejected. Reason: Phòng đã có người đặt trước.",
                Type    = "BookingRejected",
                Status  = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                SentAt  = DateTime.UtcNow.AddDays(-10)
            },
            // Cancelled notification (UC7 flow)
            new Notification
            {
                BookingId = bookings[3].BookingId,
                RecipientEmail = tenants[0].Email,
                Subject = "Booking Cancelled - Room 102",
                MessageContent = "Dear Lê Văn Cường, your booking request has been successfully cancelled.",
                Type    = "BookingCancelled",
                Status  = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                SentAt  = DateTime.UtcNow.AddDays(-18)
            },
            // Payment success notification
            new Notification
            {
                BookingId = bookings[5].BookingId,
                RecipientEmail = tenants[0].Email,
                Subject = "Payment Successful - Room A01",
                MessageContent = "Dear Lê Văn Cường, deposit payment successful. Booking confirmed.",
                Type    = "PaymentSuccess",
                Status  = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                SentAt  = DateTime.UtcNow.AddDays(-10)
            },
            // Booking Confirm notification (UC8 flow – Pending)
            new Notification
            {
                BookingId = bookings[0].BookingId,
                RecipientEmail = tenants[0].Email,
                Subject = "Booking Confirmation - Booking #1",
                MessageContent = "Dear Lê Văn Cường, your booking request has been submitted and is pending approval.",
                Type    = "BookingConfirm",
                Status  = "Sent",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                SentAt  = DateTime.UtcNow.AddDays(-2)
            }
        };
        await context.Notifications.AddRangeAsync(notifications);
        await context.SaveChangesAsync();

        // =============================================
        // 9. FAVORITES
        // =============================================
        var favorites = new List<Favorite>
        {
            new Favorite { TenantId = tenants[0].TenantId, HostelId = hostels[2].HostelId, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new Favorite { TenantId = tenants[1].TenantId, HostelId = hostels[0].HostelId, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new Favorite { TenantId = tenants[2].TenantId, HostelId = hostels[0].HostelId, CreatedAt = DateTime.UtcNow.AddDays(-10) }
        };
        await context.Favorites.AddRangeAsync(favorites);
        await context.SaveChangesAsync();

        // =============================================
        // 10. REVIEWS
        // =============================================
        var reviews = new List<Review>
        {
            new Review
            {
                TenantId  = tenants[0].TenantId,
                HostelId  = hostels[0].HostelId,
                BookingId = bookings[1].BookingId,
                Rating    = 5,
                Comment   = "Phòng sạch sẽ, chủ trọ nhiệt tình, vị trí thuận tiện!",
                OwnerReply = "Cảm ơn bạn đã tin tưởng và ủng hộ nhà trọ chúng tôi!",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new Review
            {
                TenantId  = tenants[2].TenantId,
                HostelId  = hostels[0].HostelId,
                BookingId = null,
                Rating    = 4,
                Comment   = "Khu vực an ninh, giá hợp lý. Nên cải thiện thêm chỗ để xe.",
                OwnerReply = null,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };
        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();

        // =============================================
        // 11. ROOM UPDATE LOGS
        // =============================================
        var logs = new List<RoomUpdateLog>
        {
            new RoomUpdateLog
            {
                RoomId = rooms[0].RoomId,
                BookingId = bookings[1].BookingId,
                ChangedByOwnerId = owners[0].OwnerId,
                StatusBefore = "Available",
                StatusAfter  = "Occupied",
                ChangedAt = DateTime.UtcNow.AddDays(-60)
            }
        };
        await context.RoomUpdateLogs.AddRangeAsync(logs);
        await context.SaveChangesAsync();
    }
}
