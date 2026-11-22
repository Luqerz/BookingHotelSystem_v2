using BookingHotelSystem.Data;
using BookingHotelSystem.Filters;
using BookingHotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Controllers
{
    // satu page khas untuk Admin sahaja
    //   Tab 1 - Reports : jana laporan tempahan (mingguan, bulanan, julat bulan, tarikh tersuai)
    //   Tab 2 - Rooms   : urus bilik (tambah bilik baru, edit maklumat bilik)
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------- Halaman Admin Panel + jana laporan ----------
        public async Task<IActionResult> Index(
            string? tab, string? type, string? basis,
            DateTime? date, int? year, int? month, int? fromMonth, int? toMonth,
            DateTime? fromDate, DateTime? toDate)
        {
            var vm = new AdminPanelViewModel();
            vm.Rooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();

            // Tab mana yang dibuka: kalau tekan Generate = tab laporan, kalau balik dari edit bilik = tab bilik
            vm.ActiveTab = (type == null && tab == "rooms") ? "rooms" : "report";

            // Kalau belum tekan Generate, akan keluar page kosong sahaja
            if (type == null) return View(vm);

            // Isi semula borang
            vm.Type = type;
            vm.Basis = basis == "checkin" ? "checkin" : "created";
            vm.Date = date ?? vm.Date;
            vm.Year = year ?? vm.Year;
            vm.Month = month ?? vm.Month;
            vm.FromMonth = fromMonth ?? vm.FromMonth;
            vm.ToMonth = toMonth ?? vm.ToMonth;
            vm.FromDate = fromDate ?? vm.FromDate;
            vm.ToDate = toDate ?? vm.ToDate;

            if (vm.Year < 2000 || vm.Year > 2100 ||
                vm.Month < 1 || vm.Month > 12 ||
                vm.FromMonth < 1 || vm.FromMonth > 12 ||
                vm.ToMonth < 1 || vm.ToMonth > 12)
            {
                vm.Error = "Please enter a valid year and month.";
                return View(vm);
            }

            // 1. Tentukan tarikh mula dan tarikh tamat (tamat = hari selepas hari terakhir)
            DateTime start, end;
            string title;

            switch (vm.Type)
            {
                case "weekly":
                    // Mingguan: Isnin hingga Ahad bagi minggu yang mengandungi tarikh dipilih
                    int diff = ((int)vm.Date.DayOfWeek + 6) % 7; // Isnin = 0
                    start = vm.Date.Date.AddDays(-diff);
                    end = start.AddDays(7);
                    title = $"Weekly Report ({start:dd MMM yyyy} - {end.AddDays(-1):dd MMM yyyy})";
                    break;

                case "monthly":
                    start = new DateTime(vm.Year, vm.Month, 1);
                    end = start.AddMonths(1);
                    title = $"Monthly Report ({start:MMMM yyyy})";
                    break;

                case "range":
                    // Julat bulan, contoh: Januari hingga Jun
                    if (vm.ToMonth < vm.FromMonth)
                    {
                        vm.Error = "'To month' must be the same as or after 'From month'.";
                        return View(vm);
                    }
                    start = new DateTime(vm.Year, vm.FromMonth, 1);
                    end = new DateTime(vm.Year, vm.ToMonth, 1).AddMonths(1);
                    title = $"Report {start:MMMM} - {end.AddDays(-1):MMMM yyyy}";
                    break;

                case "custom":
                    if (vm.ToDate.Date < vm.FromDate.Date)
                    {
                        vm.Error = "'To date' must be the same as or after 'From date'.";
                        return View(vm);
                    }
                    start = vm.FromDate.Date;
                    end = vm.ToDate.Date.AddDays(1);
                    title = $"Report ({start:dd MMM yyyy} - {end.AddDays(-1):dd MMM yyyy})";
                    break;

                default:
                    vm.Error = "Please choose a report type.";
                    return View(vm);
            }

            // 2. Ambil tempahan dalam tempoh tu
            bool byCheckIn = vm.Basis == "checkin";
            var query = _context.Bookings.Include(b => b.Room).Include(b => b.User).AsQueryable();

            if (byCheckIn)
                query = query.Where(b => b.CheckInDate >= start && b.CheckInDate < end);
            else
                query = query.Where(b => b.CreatedAt >= start && b.CreatedAt < end);

            var list = await query.OrderBy(b => b.Id).ToListAsync();

            // 3. Kira jumlah
            var report = new ReportResult
            {
                Title = title,
                BasisLabel = byCheckIn ? "Check-in date" : "Booking date",
                TotalBookings = list.Count,
                CancelledBookings = list.Count(b => b.Status == BookingStatus.Cancelled),
                Revenue = list.Where(b => BookingStatus.CountsAsRevenue(b.Status)).Sum(b => b.TotalPrice),
                Bookings = list
            };
            report.ActiveBookings = report.TotalBookings - report.CancelledBookings;

            foreach (var s in BookingStatus.All)
            {
                report.StatusCounts[s] = list.Count(b => b.Status == s);
            }

            // 4. Jadual ringkasan: kumpul ikut hari (tempoh pendek) atau ikut bulan (tempoh panjang)
            bool groupByMonth = (end - start).TotalDays > 62;
            report.GroupLabel = groupByMonth ? "Month" : "Day";

            report.Rows = list
                .GroupBy(b =>
                {
                    var dt = byCheckIn ? b.CheckInDate : b.CreatedAt;
                    return groupByMonth ? new DateTime(dt.Year, dt.Month, 1) : dt.Date;
                })
                .OrderBy(g => g.Key)
                .Select(g => new ReportRow
                {
                    Label = groupByMonth ? g.Key.ToString("MMMM yyyy") : g.Key.ToString("ddd, dd MMM yyyy"),
                    Bookings = g.Count(),
                    Cancelled = g.Count(b => b.Status == BookingStatus.Cancelled),
                    Revenue = g.Where(b => BookingStatus.CountsAsRevenue(b.Status)).Sum(b => b.TotalPrice)
                })
                .ToList();

            vm.Report = report;
            return View(vm);
        }

        // ---------- Tambah bilik baru ----------
        public IActionResult CreateRoom()
        {
            return View(new Room());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom([Bind("RoomNumber,RoomType,Price,IsAvailable")] Room room)
        {
            // Nombor bilik tak boleh sama
            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber))
            {
                ModelState.AddModelError("RoomNumber", "This room number already exists.");
            }

            if (!ModelState.IsValid) return View(room);

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Room added.";
            return RedirectToAction(nameof(Index), new { tab = "rooms" });
        }

        // ---------- Edit maklumat bilik ----------
        public async Task<IActionResult> EditRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, [Bind("Id,RoomNumber,RoomType,Price,IsAvailable")] Room room)
        {
            if (id != room.Id) return NotFound();

            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber && r.Id != room.Id))
            {
                ModelState.AddModelError("RoomNumber", "This room number already exists.");
            }

            if (!ModelState.IsValid) return View(room);

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Room updated.";
            return RedirectToAction(nameof(Index), new { tab = "rooms" });
        }
    }
}
