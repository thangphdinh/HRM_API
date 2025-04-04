using HRM_API.Models.Requests;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM_API.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;

        public UserController(IUserService userService, IOrganizationService organizationService)
        {
            _userService = userService;
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }
            if(currentUser.Data.Role == "SystemAdmin")
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users.Data);   
            }
            else if (currentUser.Data.Role == "Admin")
            {
                var organizationResult= await _organizationService.GetOrganizationInforByUserIdAsync(currentUser.Data.UserId);
                var users = await _organizationService.GetUsersByOrganizationAsync(organizationResult.Data.OrganizationId);
                return Ok(users.Data);
            }
            else if (currentUser.Data.Role == "Member")
            {
                return Forbid();
            }
            return Unauthorized();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }
            if (currentUser.Data.Role == "Member")
            {
                return Forbid();
            }
            var organizationOfCurrentUser = await _organizationService.GetOrganizationInforByUserIdAsync(currentUser.Data.UserId);
            if (!organizationOfCurrentUser.Success)
            {
                return BadRequest(organizationOfCurrentUser.ErrorMessage);
            }
            if (currentUser.Data.Role == "SystemAdmin" || (currentUser.Data.Role == "Admin" && organizationOfCurrentUser.Data.OrganizationId == request.OrganizationId))
            {
                var result = await _userService.CreateUserAsync(request);
                if (result.Success)
                {
                    // Trả về mã trạng thái 201 Created với thông tin người dùng mới được tạo
                    return CreatedAtAction(nameof(GetAllUsers), new { id = result.Data.UserId }, result.Data);
                }
                else
                {
                    return BadRequest(result.ErrorMessage);
                }
            }
            return Unauthorized();
        }
    }
}
