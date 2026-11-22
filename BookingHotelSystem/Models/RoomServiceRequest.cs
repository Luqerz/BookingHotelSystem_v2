using System.ComponentModel.DataAnnotations;

namespace BookingHotelSystem.Models
{
    // Jenis servis yang boleh dipilih pelanggan
    public static class ServiceTypes
    {
        public static readonly string[] All = { "Housekeeping", "Food & Beverage", "Extra Amenities" };
    }

    // Status permintaan servis bilik
    public static class RoomServiceStatus
    {
        public const string Pending = "Pending";
        public const string InProgress = "In Progress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All = { Pending, InProgress, Completed, Cancelled };
    }

    // Jadual RoomServiceRequests: permintaan servis bilik oleh pelanggan
    public class RoomServiceRequest
    {
        public int Id { get; set; }

        // Permintaan ini untuk tempahan (bilik) yang mana
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        // Pelanggan yang membuat permintaan
        public int UserId { get; set; }
        public User? User { get; set; }

        public string ServiceType { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Details { get; set; } = string.Empty;

        public string Status { get; set; } = RoomServiceStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
