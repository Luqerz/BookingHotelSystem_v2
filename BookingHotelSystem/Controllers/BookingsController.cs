using BookingHotelSystem.Data;
using BookingHotelSystem.Filters;
using BookingHotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Controllers
{
    // Bahagian PELANGGAN: buat tempahan, bayar, lihat resit, batal tempahan sendiri.
    // Urusan staf/admin (check-in, check-out, semua tempahan) ada di BookingManagementController.
    [LoginRequired]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Siapa yang sedang login (diambil dari session)
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        // Ambil satu tempahan milik user yang sedang login. Kalau bukan miliknya, return null.
        private async Task<Booking?> GetMyBooking(int id)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId);
        }

        // ---------- Senarai tempahan saya ----------
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == CurrentUserId)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            return View(bookings);
        }

        // ---------- Buat tempahan ----------
        public async Task<IActionResult> Create(int? roomId)
        {
            return await ShowCreateForm(roomId, DateTime.Today, DateTime.Today.AddDays(1), null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = await _context.Rooms.FindAsync(roomId);

            // Semakan sebelum simpan
            string? error = null;
            if (room == null || !room.IsAvailable)
                error = "Please select a valid room.";
            else if (checkIn.Date < DateTime.Today)
                error = "Check-in date cannot be in the past.";
            else if (checkOut.Date <= checkIn.Date)
                error = "Check-out date must be after check-in date.";
            else
            {
                // Semak bilik tu dah ditempah ke belum pada tarikh yang bertindih
                bool taken = await _context.Bookings.AnyAsync(b =>
                    b.RoomId == roomId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.CheckInDate < checkOut.Date &&
                    b.CheckOutDate > checkIn.Date);

                if (taken) error = "Sorry, this room is already booked for the selected dates.";
            }

            if (error != null || room == null)
            {
                return await ShowCreateForm(roomId, checkIn, checkOut, error);
            }

            // Jumlah harga = bilangan malam x harga bilik
            int nights = (checkOut.Date - checkIn.Date).Days;

            var booking = new Booking
            {
                UserId = CurrentUserId,
                RoomId = roomId,
                CheckInDate = checkIn.Date,
                CheckOutDate = checkOut.Date,
                TotalPrice = nights * room.Price,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Payment), new { id = booking.Id });
        }

        // Papar borang tempahan (digunakan di dua tempat di atas)
        private async Task<IActionResult> ShowCreateForm(int? roomId, DateTime checkIn, DateTime checkOut, string? error)
        {
            ViewBag.Rooms = await _context.Rooms.Where(r => r.IsAvailable).OrderBy(r => r.RoomNumber).ToListAsync();
            ViewBag.RoomId = roomId;
            ViewBag.CheckIn = checkIn.ToString("yyyy-MM-dd");
            ViewBag.CheckOut = checkOut.ToString("yyyy-MM-dd");
            ViewBag.Error = error;
            return View("Create");
        }

        // ---------- Bayaran ----------
        public async Task<IActionResult> Payment(int id)
        {
            var booking = await GetMyBooking(id);
            if (booking == null) return NotFound();

            // Hanya tempahan "Pending" perlu bayar
            if (booking.Status != BookingStatus.Pending) return RedirectToAction(nameof(Index));

            return View(booking);
        }

        // Ini bayaran simulasi sahaja (belum sambung ke payment gateway sebenar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var booking = await GetMyBooking(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.Paid;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Receipt), new { id = booking.Id });
        }

        // ---------- Resit ----------
        public async Task<IActionResult> Receipt(int id)
        {
            var booking = await GetMyBooking(id);
            if (booking == null) return NotFound();

            // Belum bayar atau dah batal = tiada resit
            if (booking.Status == BookingStatus.Pending) return RedirectToAction(nameof(Payment), new { id });
            if (booking.Status == BookingStatus.Cancelled) return RedirectToAction(nameof(Index));

            return View(booking);
        }

        // ---------- Batal tempahan ----------
        // Hanya boleh batal sebelum check-in. Rekod tak dipadam supaya ada sejarah.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await GetMyBooking(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Paid)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking cancelled.";
            }
            else
            {
                TempData["Error"] = "This booking can no longer be cancelled.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
