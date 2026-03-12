namespace DuplicateDocsFinder.ServiceResult
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }


    public static class ServiceResponse
    {
        public static ServiceResult<T> Success<T>(T data, string message = "Success", int statusCode = 200)
        {
            return new ServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult<T> Fail<T>(string message, int statusCode = 400)
        {
            return new ServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}
