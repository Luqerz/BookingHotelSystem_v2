# Nota Ringkas - Booking Hotel System

## PENTING (kalau awak ada projek versi lama)
Struktur database dah berubah. Sebelum run, PADAM fail `hotel.db`
(dalam folder BookingHotelSystem/BookingHotelSystem). Ia dibuat semula automatik.

## Cara jalankan
1. Buka BookingHotelSystem.slnx dalam Visual Studio.
2. Tekan F5.

## Akaun lalai
| Role  | Email            | Password |
|-------|------------------|----------|
| Admin | admin@hotel.com  | admin123 |
| Staff | staff@hotel.com  | staff123 |
Pelanggan: daftar sendiri di halaman Register.

## Modul
1. Admin Panel (Admin sahaja)   : /Admin
   - Tab Reports: mingguan, bulanan, julat bulan (cth Jan-Jun), tarikh tersuai
   - Tab Manage Rooms: tambah bilik, edit maklumat bilik
2. Booking Management (Staf/Admin): /BookingManagement
   - Cari & tapis ikut nama, status, julat tarikh check-in, hari tertentu
   - Check-In, Check-Out, Batal tempahan
3. Room Service
   - Pelanggan (/RoomService)        : buat permintaan (hanya selepas Check-In)
   - Staf/Admin (/RoomService/Manage): pantau & kemas kini status

## Aliran status tempahan
Pending -> Paid -> Checked-In -> Checked-Out   (atau Cancelled sebelum check-in)

## Struktur folder
- Models/ Data/ Controllers/ Views/ Filters/ Helpers/

## Reset database
Tutup aplikasi, padam `hotel.db`, jalankan semula.
Buat begini juga kalau awak ubah Model (tambah column dsb).
