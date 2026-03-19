namespace QuanLyNhanVien.Models.Common
{
    /// <summary>
    /// ARCH-6: Result pattern thống nhất — thay thế tuple (bool, string) và (bool, string, int)
    /// </summary>
    public class Result
    {
        public bool Success { get; }
        public string Message { get; }

        protected Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static Result Ok(string message = "") => new(true, message);
        public static Result Fail(string message) => new(false, message);

        public static Result<T> Ok<T>(T data, string message = "") => new(data, true, message);
        public static Result<T> Fail<T>(string message) => new(default!, false, message);
    }

    /// <summary>
    /// Result có chứa data trả về
    /// </summary>
    public class Result<T> : Result
    {
        public T Data { get; }

        internal Result(T data, bool success, string message) : base(success, message)
        {
            Data = data;
        }
    }
}
