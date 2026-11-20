namespace KWMS.Controllers
{




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