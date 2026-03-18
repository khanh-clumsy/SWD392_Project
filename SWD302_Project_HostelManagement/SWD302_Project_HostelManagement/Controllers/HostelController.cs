
﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;

using SWD302_Project_HostelManagement.ViewModels;

namespace SWD302_Project_HostelManagement.Controllers
{

    
    

    public class HostelController : Controller
    {
        private readonly AppDbContext _context;

        public HostelController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Search(string? keywords, string? address, decimal? minPrice, decimal? maxPrice, string? status)
        {
            var filters = new HostelSearchFilterViewModel
            {
                Keywords = keywords,
                Address = address,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Status = status
            };

            var hostelQuery = _context.Hostels
                .AsNoTracking()
                .Include(h => h.Rooms)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                var normalizedKeywords = keywords.Trim().ToLower();
                hostelQuery = hostelQuery.Where(h =>
                    h.Name.ToLower().Contains(normalizedKeywords) ||
                    h.Address.ToLower().Contains(normalizedKeywords) ||
                    (h.Description != null && h.Description.ToLower().Contains(normalizedKeywords)));
            }

            if (!string.IsNullOrWhiteSpace(address))
            {
                var normalizedAddress = address.Trim().ToLower();
                hostelQuery = hostelQuery.Where(h => h.Address.ToLower().Contains(normalizedAddress));
            }

            if (minPrice.HasValue)
            {
                hostelQuery = hostelQuery.Where(h => h.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                hostelQuery = hostelQuery.Where(h => h.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                hostelQuery = hostelQuery.Where(h => h.Status == status);
            }
            else
            {
                hostelQuery = hostelQuery.Where(h => h.Status == "Approved");
            }

            var hostelResults = await hostelQuery
                .Select(h => new HostelSearchResultViewModel
                {
                    HostelId = h.HostelId,
                    Name = h.Name,
                    Address = h.Address,
                    Price = h.Price,
                    Status = h.Status,
                    Description = h.Description,
                    AvailableRoomCount = h.Rooms.Count(r => r.Status == "Available")
                })
                .Where(h => h.AvailableRoomCount > 0)
                .OrderBy(h => h.Name)
                .ToListAsync();

            var viewModel = new HostelSearchPageViewModel
            {
                Filters = filters,
                Results = hostelResults,
                Message = hostelResults.Count == 0
                    ? "No hostels match the current search criteria."
                    : null
            };

            return View(viewModel);
        }

        /// <summary>
        /// UC09 — Add New Hostel Property
        /// Boundary  : HostelInteraction  (Views/Hostel/*)
        /// Controller: HostelCoordinator  (this class)
        /// Entity    : Hostel             (EF model)
        /// </summary>
        // ─────────────────────────────────────────────────────────────
        // HELPER: đọc OwnerId từ Cookie Claims
        // AuthController lưu: new Claim(ClaimTypes.NameIdentifier, owner.OwnerId.ToString())
        // ─────────────────────────────────────────────────────────────
        [Authorize(Roles = "HostelOwner")]
        private int GetCurrentOwnerId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }

        // ─────────────────────────────────────────────────────────────
        // INDEX — danh sách hostel của owner đang đăng nhập
        // ─────────────────────────────────────────────────────────────
        [Authorize(Roles = "HostelOwner")]
        public async Task<IActionResult> Index()
        {
            var ownerId = GetCurrentOwnerId();

            var hostels = await _context.Hostels
                .Where(h => h.OwnerId == ownerId)
                .OrderByDescending(h => h.CreatedDate)
                .ToListAsync();

            return View(hostels);
        }

        // ─────────────────────────────────────────────────────────────
        // UC09 — CREATE GET: hiển thị Add Hostel Form
        // PRE-1: Owner đã đăng nhập — đảm bảo bởi [Authorize] phía trên
        // ─────────────────────────────────────────────────────────────
        [Authorize(Roles = "HostelOwner")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new HostelCreateViewModel());
        }

        // ─────────────────────────────────────────────────────────────
        // UC09 — CREATE POST: HostelCoordinator.CreateHostel()
        //
        // P2  — nhận dữ liệu từ Boundary (form)
        // P3  — validate ModelState              → AS 9.1 nếu invalid
        // P4  — checkDuplicateHostel             → AS 9.2 nếu trùng
        // P6  — tạo Hostel, status=PendingApproval
        // P7  — lưu DB
        // P8  — redirect + TempData success
        //
        // AS 9.1 Validation Failure : ModelState invalid
        // AS 9.2 Duplicate Hostel   : trùng Name+Address cùng Owner
        // AS 9.3 Cancellation       : nút Cancel ở View → GET /Hostel/Index
        // EX 9.1 Session/Auth hết   : [Authorize] tự redirect /Auth/Login
        // ─────────────────────────────────────────────────────────────
        [Authorize(Roles = "HostelOwner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HostelCreateViewModel vm)
        {
            // AS 9.1 — Validation Failure (P3)
            if (!ModelState.IsValid)
                return View(vm);

            var ownerId = GetCurrentOwnerId();

            // AS 9.2 — Duplicate Hostel Check (P4)
            // Tiêu chí: cùng OwnerId + Name + Address (không phân biệt hoa thường)
            bool isDuplicate = await _context.Hostels.AnyAsync(h =>
                h.OwnerId == ownerId &&
                h.Name.ToLower() == vm.Name.Trim().ToLower() &&
                h.Address.ToLower() == vm.Address.Trim().ToLower() &&
                h.Status != "Deleted");

            if (isDuplicate)
            {
                // P4A → P5A — báo lỗi duplicate về View
                ModelState.AddModelError(string.Empty,
                    "Bạn đã có hostel với tên và địa chỉ này. Vui lòng kiểm tra lại.");
                return View(vm);
            }

            // P6 — tạo Hostel entity, status = PendingApproval (POST-1, POST-2)
            var hostel = new Hostel
            {
                OwnerId = ownerId,
                Name = vm.Name.Trim(),
                Address = vm.Address.Trim(),
                Description = vm.Description?.Trim(),
                Status = "PendingApproval",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            // P7 — lưu DB
            _context.Hostels.Add(hostel);
            await _context.SaveChangesAsync();

            // P8 — POST-3: thông báo thành công
            TempData["Success"] =
                $"Hostel \"{hostel.Name}\" đã được tạo và đang chờ Admin duyệt.";

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────
        // DETAILS — xem hostel + danh sách phòng (entry point cho UC10)
        // ─────────────────────────────────────────────────────────────
        [Authorize(Roles = "HostelOwner")]
        public async Task<IActionResult> Details(int id)
        {
            var ownerId = GetCurrentOwnerId();

            var hostel = await _context.Hostels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HostelId == id && h.OwnerId == ownerId);

            if (hostel == null)
                return NotFound();

            return View(hostel);
        }
    }
}
