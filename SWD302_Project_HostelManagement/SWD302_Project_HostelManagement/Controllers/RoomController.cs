using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;

namespace SWD302_Project_HostelManagement.Controllers
{
    public class RoomController : Controller
    {
        private readonly AppDbContext _context;

        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _context.Rooms
                .AsNoTracking()
                .Include(r => r.Hostel)
                .Include(r => r.Owner)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }
    }
}
