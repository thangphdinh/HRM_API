using HRM_API.Models.Requests;
using HRM_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

    }
}
