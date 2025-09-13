namespace UniDownload.UniDownloadCore
{
    internal class Result<T>
    {
        public T Value { get; }
        public bool IsSuccess { get; }
        public string Message { get; }
        
        public Result(T value)
        {
            Value = value;
            IsSuccess = true;
        }

        public Result(string message)
        {
            IsSuccess = false;
            Message = message;
            Value = default;
            UniLogger.Error(message);
        }

        public static Result<T> Success(T value) => new Result<T>(value);
        public static Result<T> Fail(string message) => new Result<T>(message);
    }
}