using BookingHotelSystem.Helpers;
using BookingHotelSystem.Models;

namespace BookingHotelSystem.Data
{
    // Isi data asas ke dalam database kali pertama sahaja.
    // Kalau data dah ada, tak akan ditambah lagi.
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            // Akaun admin lalai: admin@hotel.com / admin123 (tukar selepas login pertama kalau perlu)
            if (!db.Users.Any())
            {
                db.Users.Add(new User
                {
                    Name = "Admin",
                    Email = "admin@hotel.com",
                    PasswordHash = PasswordHelper.Hash("admin123"),
                    Role = "Admin"
                });
            }

            // Akaun staf lalai: staff@hotel.com / staff123
            // Staf boleh urus tempahan dan servis bilik, tapi tak boleh masuk Admin Panel.
            if (!db.Users.Any(u => u.Role == "Staff"))
            {
                db.Users.Add(new User
                {
                    Name = "Front Desk Staff",
                    Email = "staff@hotel.com",
                    PasswordHash = PasswordHelper.Hash("staff123"),
                    Role = "Staff"
                });
            }

            // Bilik contoh supaya sistem tak nampak kosong
            if (!db.Rooms.Any())
            {
                db.Rooms.AddRange(
                    new Room { RoomNumber = "101", RoomType = "Standard", Price = 120 },
                    new Room { RoomNumber = "102", RoomType = "Standard", Price = 120 },
                    new Room { RoomNumber = "201", RoomType = "Deluxe", Price = 220 },
                    new Room { RoomNumber = "202", RoomType = "Deluxe", Price = 220 },
                    new Room { RoomNumber = "301", RoomType = "Suite", Price = 450 }
                );
            }

            db.SaveChanges();
        }
    }
}
