using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
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
    }
}
