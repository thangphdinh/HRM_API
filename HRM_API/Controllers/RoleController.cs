using HRM_API.Common.Enums;
using HRM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HRM_API.Controllers
{
    [Authorize]
    [Route("api/roles")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        public RoleController(IRoleService roleService, IUserService userService)
        {
            _roleService = roleService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!currentUser.Success)
            {
                return Unauthorized();
            }

            if (!Enum.TryParse<RoleEnum>(currentUser.Data.Role, out var roleEnum))
                return Forbid();

            var roles = await _roleService.GetAllRolesAsync();
            if (!roles.Success)
            {
                return Unauthorized();
            }
            switch (roleEnum)
            {
                case RoleEnum.SystemAdmin: return Ok(roles.Data);

                case RoleEnum.Admin:
                    var filterRoles = roles.Data.Where(r => r.RoleName != RoleEnum.SystemAdmin.ToString()).ToList();
                    return Ok(filterRoles);

                case RoleEnum.Member: default: return Forbid();
            }
        }
    }
}
