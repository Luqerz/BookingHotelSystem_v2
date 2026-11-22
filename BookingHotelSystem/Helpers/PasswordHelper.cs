using System.Security.Cryptography;

namespace BookingHotelSystem.Helpers
{
    // Simpan password dalam bentuk "hash" supaya orang tak nampak password sebenar dalam database.
    public static class PasswordHelper
    {
        // Tukar password biasa kepada hash (ada "salt" rawak di dalamnya)
        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        // Semak sama ada password yang ditaip sepadan dengan hash dalam database
        public static bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expected = Convert.FromBase64String(parts[1]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
    }
}
