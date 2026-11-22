using BookingHotelSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingHotelSystem.Data
{
    // Kelas ini yang berhubung terus dengan database.
    // Setiap DbSet = satu jadual dalam database.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<RoomServiceRequest> RoomServiceRequests { get; set; }
    }
}
