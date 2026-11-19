using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace KWMS.Controllers
{
    /// <summary>Reusable input checks. Each method answers one yes/no question.</summary>
    public static class InputValidator
    {
        // Letters, digits and underscore only, 3 to 30 characters
        public static bool IsValidUserName(string value) =>
            Regex.IsMatch(value, "^[A-Za-z0-9_]{3,30}$");

        public static bool IsValidEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 100) return false;
            try
            {
                MailAddress address = new MailAddress(value);
                return address.Address == value && address.Host.Contains('.');
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // 8 to 72 bytes (BCrypt ignores everything after 72 bytes), at least one letter and one digit
        public static bool IsValidPassword(string value) =>
            value.Length >= 8
            && Encoding.UTF8.GetByteCount(value) <= 72
            && value.Any(char.IsLetter)
            && value.Any(char.IsDigit);

        // Optional leading +, then 9 to 15 digits. Spaces are ignored.
        public static bool IsValidPhone(string value) =>
            Regex.IsMatch(value.Replace(" ", ""), @"^\+?[0-9]{9,15}$");
    }
}