namespace KWMS.Controllers
{
    /// <summary>
    /// What a controller returns when the View asked for DATA (for example a list of companies).
    /// Success = false means Data must not be used, and Message explains why.
    /// </summary>
    public class DataResult<T>
    {
        public bool Success { get; }
        public string Message { get; }
        public T Data { get; }

        private DataResult(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static DataResult<T> Ok(T data) => new DataResult<T>(true, string.Empty, data);
        public static DataResult<T> Fail(string message) => new DataResult<T>(false, message, default!);
    }
}