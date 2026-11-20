namespace KWMS.Controllers
{

    public static class PasswordHasher
    {

        private const int WorkFactor = 11;

        public static string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);






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