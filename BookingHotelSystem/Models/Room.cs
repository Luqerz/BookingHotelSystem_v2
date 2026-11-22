using System.ComponentModel.DataAnnotations;

namespace BookingHotelSystem.Models
{
    // Jadual Rooms: senarai bilik hotel
    public class Room
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Room number is required")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room type is required")]
        public string RoomType { get; set; } = string.Empty; // Standard, Deluxe, Suite

        [Range(0.01, 10000, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; } // harga satu malam (RM)

        // Admin boleh matikan bilik (contoh: sedang diselenggara) supaya tak boleh ditempah
        public bool IsAvailable { get; set; } = true;
    }
}
