using BookingHotelSystem.Data;
using BookingHotelSystem.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Controllers
{
    // Halaman senarai bilik untuk pelanggan (lihat, cari, tempah).
    // Tambah/edit bilik dibuat oleh Admin di Admin Panel.
    [LoginRequired]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var rooms = _context.Rooms.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                rooms = rooms.Where(r => r.RoomNumber.Contains(searchString) || r.RoomType.Contains(searchString));
            }

            ViewBag.Search = searchString;
            return View(await rooms.OrderBy(r => r.RoomNumber).ToListAsync());
        }
    }
}
