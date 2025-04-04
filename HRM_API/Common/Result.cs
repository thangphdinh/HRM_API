namespace HRM_API.Common
{
    public class Result<T>
    {
        public bool Success { get; }
        public T Data { get; }
        public string ErrorMessage { get; }
        public int? ErrorCode { get; }

        // Constructor cho trường hợp thành công
        private Result(bool success, T data, string errorMessage, int? errorCode)
        {
            Success = success;
            Data = data;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        // Phương thức trả về kết quả thành công
        public static Result<T> SuccessResult(T data)
        {
            return new Result<T>(true, data, null, null);
        }

        // Phương thức trả về kết quả thất bại
        public static Result<T> FailureResult(string errorMessage, int? errorCode = null)
        {
            return new Result<T>(false, default(T), errorMessage, errorCode);
        }
    }

}
