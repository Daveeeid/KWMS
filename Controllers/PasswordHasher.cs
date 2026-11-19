namespace KWMS.Controllers
{
    /// <summary>Wraps BCrypt so the rest of the app never touches the library directly.</summary>
    public static class PasswordHasher
    {
        // Work factor 11 = 2^11 rounds. Higher is slower and safer; 11 is fast enough for a desktop app.
        private const int WorkFactor = 11;

        public static string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        /// <summary>
        /// Returns true if the password matches the stored hash.
        /// If the stored value is not a valid BCrypt hash (for example, plaintext someone typed
        /// into the table by hand), Verify throws, and we simply return false.
        /// </summary>
        public static bool Verify(string password, string storedHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}