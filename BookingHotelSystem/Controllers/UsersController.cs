using BookingHotelSystem.Data;
using BookingHotelSystem.Filters;
using BookingHotelSystem.Helpers;
using BookingHotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------- Senarai semua user (Admin sahaja) ----------
        [AdminOnly]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.OrderBy(u => u.Id).ToListAsync());
        }

        // ---------- Tukar role user (Admin sahaja): User / Staff / Admin ----------
        [AdminOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var validRoles = new[] { "User", "Staff", "Admin" };
            if (!validRoles.Contains(role)) return RedirectToAction(nameof(Index));

            // Admin tak boleh tukar role sendiri (nanti terkunci keluar)
            if (id == HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{user.Name} is now {role}.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Register ----------
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            // Semak input kosong
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please fill in all fields.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters.";
                return View();
            }

            // Semak email dah digunakan atau belum
            email = email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "This email is already registered.";
                return View();
            }

            // Simpan user baru. Semua yang register sendiri ialah "User" biasa (bukan Admin)
            var user = new User
            {
                Name = name.Trim(),
                Email = email,
                PasswordHash = PasswordHelper.Hash(password),
                Role = "User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful. Please login.";
            return RedirectToAction(nameof(Login));
        }

        // ---------- Login ----------
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            email = (email ?? "").Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Kalau user tak wujud atau password salah
            if (user == null || !PasswordHelper.Verify(password ?? "", user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // Simpan maklumat dalam session supaya sistem tahu siapa yang login
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("Role", user.Role);

            // Hantar ke halaman yang sesuai ikut role
            if (user.Role == "Admin") return RedirectToAction("Index", "Admin");
            if (user.Role == "Staff") return RedirectToAction("Index", "BookingManagement");
            return RedirectToAction("Index", "Rooms");
        }

        // ---------- Logout ----------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
