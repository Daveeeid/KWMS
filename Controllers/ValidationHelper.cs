namespace KWMS.Controllers
{
    /// <summary>Small shared helpers for the controllers added from Stage 5 onward.</summary>
    public static class ValidationHelper
    {
        // Empty or whitespace-only text becomes null, so the database stores NULL instead of ''
        public static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        // Turns a list of problems into one bulleted message
        public static string FormatErrors(List<string> errors) =>
            string.Join(Environment.NewLine, errors.Select(e => "• " + e));
    }
}