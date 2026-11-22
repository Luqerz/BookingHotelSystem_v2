namespace BookingHotelSystem.Models
{
    // Senarai status tempahan. Guna pemalar (const) supaya tak tersalah eja di tempat lain.
    // Aliran biasa:  Pending -> Paid -> Checked-In -> Checked-Out   (atau Cancelled)
    public static class BookingStatus
    {
        public const string Pending = "Pending";       // belum bayar
        public const string Paid = "Paid";             // dah bayar, menunggu tarikh check-in
        public const string CheckedIn = "Checked-In";  // tetamu sedang menginap
        public const string CheckedOut = "Checked-Out";// tetamu dah keluar
        public const string Cancelled = "Cancelled";   // dibatalkan

        public static readonly string[] All = { Pending, Paid, CheckedIn, CheckedOut, Cancelled };

        // Hanya tempahan yang dah dibayar dikira sebagai pendapatan
        public static bool CountsAsRevenue(string status)
        {
            return status == Paid || status == CheckedIn || status == CheckedOut;
        }
    }

    // Jadual Bookings: tempahan bilik oleh pelanggan
    public class Booking
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int RoomId { get; set; }
        public Room? Room { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = BookingStatus.Pending;

        // Bila tempahan dibuat (digunakan untuk laporan)
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Dikira automatik (tak disimpan dalam database)
        public int Nights => (CheckOutDate - CheckInDate).Days;
    }
}
