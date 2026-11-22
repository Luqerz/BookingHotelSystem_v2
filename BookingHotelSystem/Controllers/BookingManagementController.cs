using BookingHotelSystem.Data;
using BookingHotelSystem.Filters;
using BookingHotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Controllers
{
    // MODUL BOOKING MANAGEMENT (Staf dan Admin sahaja)
    // - Lihat semua tempahan pelanggan
    // - Cari / tapis ikut nama, status, julat tarikh atau hari tertentu
    // - Kemas kini status: Check-In, Check-Out, atau Batal
    [StaffOnly]
    public class BookingManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------- Senarai + carian + tapisan ----------
        // search       : nama / email tetamu atau nombor bilik
        // status       : tapis ikut status tempahan
        // fromDate/toDate : tempahan yang check-in dalam julat tarikh ini
        // day          : tetamu yang sedang menginap pada hari ini (check-in <= hari < check-out)
        public async Task<IActionResult> Index(string? search, string? status, DateTime? fromDate, DateTime? toDate, DateTime? day)
        {
            var bookings = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                bookings = bookings.Where(b =>
                    b.User!.Name.Contains(search) ||
                    b.User!.Email.Contains(search) ||
                    b.Room!.RoomNumber.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                bookings = bookings.Where(b => b.Status == status);
            }

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                bookings = bookings.Where(b => b.CheckInDate >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date;
                bookings = bookings.Where(b => b.CheckInDate <= end);
            }

            if (day.HasValue)
            {
                var d = day.Value.Date;
                bookings = bookings.Where(b => b.CheckInDate <= d && b.CheckOutDate > d);
            }

            // Hantar nilai tapisan semula ke View supaya borang tak kosong
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Day = day?.ToString("yyyy-MM-dd");

            var list = await bookings
                .OrderByDescending(b => b.CheckInDate)
                .ThenByDescending(b => b.Id)
                .ToListAsync();

            return View(list);
        }

        // ---------- Check-In (mesti sudah bayar) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id, string? returnUrl)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Paid)
            {
                booking.Status = BookingStatus.CheckedIn;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Booking #{id} checked in.";
            }
            else
            {
                TempData["Error"] = "Only paid bookings can be checked in.";
            }

            return Back(returnUrl);
        }

        // ---------- Check-Out (mesti sedang Checked-In) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id, string? returnUrl)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.CheckedIn)
            {
                booking.Status = BookingStatus.CheckedOut;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Booking #{id} checked out.";
            }
            else
            {
                TempData["Error"] = "Only checked-in bookings can be checked out.";
            }

            return Back(returnUrl);
        }

        // ---------- Batal tempahan (hanya sebelum check-in) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? returnUrl)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Paid)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Booking #{id} cancelled.";
            }
            else
            {
                TempData["Error"] = "This booking can no longer be cancelled.";
            }

            return Back(returnUrl);
        }

        // Balik ke halaman senarai dengan tapisan yang sama (kalau ada)
        private IActionResult Back(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
