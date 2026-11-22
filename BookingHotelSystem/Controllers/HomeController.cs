using System.Diagnostics;
using BookingHotelSystem.Data;
using BookingHotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Halaman utama (boleh dilihat tanpa login)
        public async Task<IActionResult> Index()
        {
            ViewBag.RoomCount = await _context.Rooms.CountAsync(r => r.IsAvailable);
            ViewBag.BookingCount = await _context.Bookings.CountAsync(b => b.Status != BookingStatus.Cancelled);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
