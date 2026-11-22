using BookingHotelSystem.Data;
using BookingHotelSystem.Filters;
using BookingHotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Controllers
{
    // MODUL ROOM SERVICE
    // Pelanggan : buat permintaan servis (housekeeping, makanan, kemudahan tambahan) dan lihat statusnya
    // Staf/Admin: pantau semua permintaan dan kemas kini status (Pending -> In Progress -> Completed)
    [LoginRequired]
    public class RoomServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        // ================= BAHAGIAN PELANGGAN =================

        // ---------- Senarai permintaan saya ----------
        public async Task<IActionResult> Index()
        {
            var requests = await _context.RoomServiceRequests
                .Include(r => r.Booking).ThenInclude(b => b!.Room)
                .Where(r => r.UserId == CurrentUserId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(requests);
        }

        // ---------- Buat permintaan baru ----------
        public async Task<IActionResult> Create()
        {
            return await ShowCreateForm(null, null, null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookingId, string serviceType, string details)
        {
            // Pastikan tempahan ni milik user dan tetamu sedang menginap (Checked-In)
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == CurrentUserId && b.Status == BookingStatus.CheckedIn);

            string? error = null;
            if (booking == null)
                error = "Please select a room you are currently staying in.";
            else if (!ServiceTypes.All.Contains(serviceType))
                error = "Please select a service type.";
            else if (string.IsNullOrWhiteSpace(details))
                error = "Please describe what you need.";
            else if (details.Length > 300)
                error = "Details are too long (max 300 characters).";

            if (error != null)
            {
                return await ShowCreateForm(bookingId, serviceType, details, error);
            }

            var request = new RoomServiceRequest
            {
                BookingId = bookingId,
                UserId = CurrentUserId,
                ServiceType = serviceType,
                Details = details.Trim(),
                Status = RoomServiceStatus.Pending,
                RequestedAt = DateTime.Now
            };
            _context.RoomServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your request has been sent. Our staff will attend to it shortly.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ShowCreateForm(int? bookingId, string? serviceType, string? details, string? error = null)
        {
            // Hanya bilik yang sedang diduduki (Checked-In) boleh buat permintaan
            ViewBag.Bookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == CurrentUserId && b.Status == BookingStatus.CheckedIn)
                .ToListAsync();

            ViewBag.BookingId = bookingId;
            ViewBag.ServiceType = serviceType;
            ViewBag.Details = details;
            ViewBag.Error = error;
            return View("Create");
        }

        // ---------- Batal permintaan (hanya kalau masih Pending) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _context.RoomServiceRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == CurrentUserId);
            if (request == null) return NotFound();

            if (request.Status == RoomServiceStatus.Pending)
            {
                request.Status = RoomServiceStatus.Cancelled;
                request.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Request cancelled.";
            }
            else
            {
                TempData["Error"] = "Only pending requests can be cancelled.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= BAHAGIAN STAF / ADMIN =================

        // ---------- Pantau semua permintaan ----------
        [StaffOnly]
        public async Task<IActionResult> Manage(string? status, string? type)
        {
            var requests = _context.RoomServiceRequests
                .Include(r => r.User)
                .Include(r => r.Booking).ThenInclude(b => b!.Room)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                requests = requests.Where(r => r.ServiceType == type);
            }

            ViewBag.Status = status;
            ViewBag.Type = type;

            return View(await requests.OrderByDescending(r => r.Id).ToListAsync());
        }

        // ---------- Kemas kini status permintaan ----------
        [StaffOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? returnUrl)
        {
            var request = await _context.RoomServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (RoomServiceStatus.All.Contains(status))
            {
                request.Status = status;
                request.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Request #{id} is now {status}.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Manage));
        }
    }
}
