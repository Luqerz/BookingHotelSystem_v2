namespace BookingHotelSystem.Models
{
    // Satu baris dalam jadual ringkasan laporan (contoh: satu hari atau satu bulan)
    public class ReportRow
    {
        public string Label { get; set; } = string.Empty;
        public int Bookings { get; set; }
        public int Cancelled { get; set; }
        public decimal Revenue { get; set; }
    }

    // Keputusan laporan
    public class ReportResult
    {
        public string Title { get; set; } = string.Empty;
        public string BasisLabel { get; set; } = string.Empty;
        public string GroupLabel { get; set; } = "Day";   // "Day" atau "Month"
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal Revenue { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public List<ReportRow> Rows { get; set; } = new();
        public List<Booking> Bookings { get; set; } = new();
    }

    // Data untuk page Admin Panel (senarai bilik + borang laporan + keputusan laporan)
    public class AdminPanelViewModel
    {
        public string ActiveTab { get; set; } = "report";
        public List<Room> Rooms { get; set; } = new();

        // Nilai dalam borang laporan (supaya kekal selepas tekan Generate)
        public string Type { get; set; } = "monthly";   // weekly, monthly, range, custom
        public string Basis { get; set; } = "created";  // created = tarikh tempahan dibuat, checkin = tarikh check-in
        public DateTime Date { get; set; } = DateTime.Today;
        public int Year { get; set; } = DateTime.Today.Year;
        public int Month { get; set; } = DateTime.Today.Month;
        public int FromMonth { get; set; } = 1;
        public int ToMonth { get; set; } = DateTime.Today.Month;
        public DateTime FromDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime ToDate { get; set; } = DateTime.Today;

        public string? Error { get; set; }
        public ReportResult? Report { get; set; }
    }
}
