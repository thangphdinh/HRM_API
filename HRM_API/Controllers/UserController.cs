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
            if(currentUser.Role == "SystemAdmin")
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);   
            }
            else if (currentUser.Role == "Admin")
            {
                var organizationId = await _organizationService.GetOrganizationIdByUserIdAsync(currentUser.UserId);
                var users = await _organizationService.GetUsersByOrganizationAsync(organizationId);
                return Ok(users);
            }
            else if (currentUser.Role == "Member")
            {
                return Forbid();
            }
            return Unauthorized();
        }
    }
}
