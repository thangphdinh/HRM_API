using HRM_API.Models.Requests;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.Login(request);
                if (result.Success)
                {
                    return Ok(result.Data);
                }
                return BadRequest(new { message = result.ErrorMessage, errorCode = result.ErrorCode });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshToken(request.RefreshToken);
            if (result == null || !result.Success)
            {
                return Unauthorized(new { message = "Invalid Refresh Token", errorCode = 401 });
            }

            return Ok(result.Data);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var result = await _authService.Logout(request.RefreshToken);
            if (result.Success)
            {
                return Ok(new { message = "Logout successful" });
            }
            return Unauthorized(new { message = "Invalid Refresh Token", errorCode = 401 });
        }

        [HttpGet("me")]
        [Authorize] // đảm bảo chỉ người đã đăng nhập mới gọi được
        public async Task<IActionResult> GetCurrentUser()
        {
            var currentUserResult = await _userService.GetCurrentUserAsync();
            if (!currentUserResult.Success)
                return BadRequest(currentUserResult.ErrorMessage);

            return Ok(currentUserResult.Data);
        }
    }
}
