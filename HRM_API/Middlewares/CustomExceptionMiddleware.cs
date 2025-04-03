namespace HRM_API.Middlewares
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;

        public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext); // Tiếp tục với request nếu không có lỗi
            }
            catch (UnauthorizedAccessException ex)
            {
                // Ghi log lỗi nếu cần thiết
                _logger.LogError($"Unauthorized access error: {ex.Message}");

                // Trả về Unauthorized (401) cho client
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Ghi log các lỗi khác
                _logger.LogError($"An error occurred: {ex.Message}");

                // Trả về Internal Server Error (500) cho client
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred" });
            }
        }
    }
}
