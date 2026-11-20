namespace KWMS.Controllers
{

    public static class ValidationHelper
    {

        public static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();


        public static string FormatErrors(List<string> errors) =>
            string.Join(Environment.NewLine, errors.Select(e => "• " + e));
    }
}