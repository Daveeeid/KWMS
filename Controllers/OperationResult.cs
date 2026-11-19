namespace KWMS.Controllers
{
    /// <summary>
    /// What a controller returns to a View: did it work, and what message should the user see?
    /// The View never needs to know about exceptions or SQL.
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; }
        public string Message { get; }

        private OperationResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static OperationResult Ok(string message = "") => new OperationResult(true, message);
        public static OperationResult Fail(string message) => new OperationResult(false, message);
    }
}