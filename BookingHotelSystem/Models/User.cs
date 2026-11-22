using System.ComponentModel.DataAnnotations;

namespace BookingHotelSystem.Models
{
    // Jadual Users: pelanggan dan admin
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Yang disimpan ialah hash, bukan password sebenar
        public string PasswordHash { get; set; } = string.Empty;

        // "Admin" atau "User"
        public string Role { get; set; } = "User";
    }
}
