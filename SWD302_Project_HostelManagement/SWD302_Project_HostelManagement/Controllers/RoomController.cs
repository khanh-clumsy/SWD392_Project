using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.ViewModels;

namespace SWD302_Project_HostelManagement.Controllers
{
    [Authorize(Roles = "HostelOwner")]
    public class RoomController : Controller
    {
        private readonly AppDbContext _context;

        private static readonly string[] AllowedStatuses =
            { "Available", "Maintenance", "Inactive" };

        private static readonly string[] ActiveBookingStatuses =
            { "Pending", "Approved", "PendingPayment", "DepositPaid", "Confirmed" };

        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentOwnerId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }

        // =========================
        // UC10 — VIEW ROOM LIST
        // =========================
        public async Task<IActionResult> Index(int hostelId)
        {
            var ownerId = GetCurrentOwnerId();

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.HostelId == hostelId && h.OwnerId == ownerId);

            if (hostel == null)
                return NotFound();

            var rooms = await _context.Rooms
                .Where(r => r.HostelId == hostelId && r.OwnerId == ownerId)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            ViewBag.Hostel = hostel;
            return View(rooms);
        }

        // =========================
        // UC10 — ADD ROOM (GET)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create(int hostelId)
        {
            var ownerId = GetCurrentOwnerId();

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.HostelId == hostelId && h.OwnerId == ownerId);

            if (hostel == null)
                return NotFound();

            var vm = new RoomCreateViewModel
            {
                HostelId = hostelId
            };

            ViewBag.HostelName = hostel.Name;
            return View(vm);
        }

        // =========================
        // UC10 — ADD ROOM (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomCreateViewModel vm)
        {
            var ownerId = GetCurrentOwnerId();

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.HostelId == vm.HostelId && h.OwnerId == ownerId);

            if (hostel == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.HostelName = hostel.Name;
                return View(vm);
            }

            // Duplicate check
            bool isDuplicate = await _context.Rooms.AnyAsync(r =>
                r.HostelId == vm.HostelId &&
                r.RoomNumber.ToLower() == vm.RoomNumber.Trim().ToLower());

            if (isDuplicate)
            {
                ModelState.AddModelError(string.Empty,
                    "Phòng này đã tồn tại.");
                ViewBag.HostelName = hostel.Name;
                return View(vm);
            }

            var room = new Room
            {
                HostelId = vm.HostelId,
                OwnerId = ownerId,
                RoomNumber = vm.RoomNumber.Trim(),
                PricePerMonth = vm.PricePerMonth,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm phòng {room.RoomNumber}.";

            return RedirectToAction(nameof(Index), new { hostelId = vm.HostelId });
        }

        // =========================
        // UC12 — CHANGE STATUS
        // =========================
        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var ownerId = GetCurrentOwnerId();

            var room = await _context.Rooms
                .Include(r => r.Hostel)
                .FirstOrDefaultAsync(r => r.RoomId == id && r.OwnerId == ownerId);

            if (room == null)
                return RedirectToAction("Index", "Hostel");

            int activeCount = await _context.BookingRequests
                .CountAsync(b => b.RoomId == id &&
                                 ActiveBookingStatuses.Contains(b.Status));

            var vm = new RoomChangeStatusViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                HostelName = room.Hostel?.Name ?? "",
                HostelId = room.HostelId,
                CurrentStatus = room.Status,
                NewStatus = room.Status,
                AllowedStatuses = AllowedStatuses,
                HasActiveBookings = activeCount > 0,
                ActiveBookingCount = activeCount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(RoomChangeStatusViewModel vm)
        {
            if (!AllowedStatuses.Contains(vm.NewStatus))
            {
                ModelState.AddModelError("", "Trạng thái không hợp lệ.");
                vm.AllowedStatuses = AllowedStatuses;
                return View(vm);
            }

            var ownerId = GetCurrentOwnerId();

            var room = await _context.Rooms
                .Include(r => r.Hostel)
                .FirstOrDefaultAsync(r => r.RoomId == vm.RoomId && r.OwnerId == ownerId);

            if (room == null)
                return RedirectToAction("Index", "Hostel");

            if (room.Status == vm.NewStatus)
                return RedirectToAction(nameof(Index), new { hostelId = room.HostelId });

            bool isRestricted = vm.NewStatus == "Maintenance" || vm.NewStatus == "Inactive";

            if (isRestricted)
            {
                int activeCount = await _context.BookingRequests
                    .CountAsync(b => b.RoomId == vm.RoomId &&
                                     ActiveBookingStatuses.Contains(b.Status));

                if (activeCount > 0)
                {
                    ModelState.AddModelError("",
                        "Không thể đổi trạng thái do đang có booking.");

                    vm.AllowedStatuses = AllowedStatuses;
                    return View(vm);
                }
            }

            string before = room.Status;

            room.Status = vm.NewStatus;
            room.UpdatedAt = DateTime.UtcNow;

            _context.RoomUpdateLogs.Add(new RoomUpdateLog
            {
                RoomId = room.RoomId,
                ChangedByOwnerId = ownerId,
                StatusBefore = before,
                StatusAfter = vm.NewStatus,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { hostelId = room.HostelId });
        }
    }
}