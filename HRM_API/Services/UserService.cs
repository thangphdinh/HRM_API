using HRM_API.Data;
using HRM_API.Models.Entities;
using HRM_API.Models.Responses;
using HRM_API.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRM_API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationService _organizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUserRepository userRepository, IOrganizationService organizationService, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _organizationService = organizationService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            var userResponses = users.Select(u => new UserResponse
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.RoleName,
                Organization = u.Organization.OrganizationName,
                Status = u.Status
            }).ToList();
            return userResponses;
        }

        public async Task<UserResponse> GetCurrentUserAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext.User?.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (userEmail == null)
            {
                return null;
            }

            var user = await _userRepository.GetUserByEmailAsync(userEmail);

            if (user == null)
                return null;

            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.RoleName,
                Organization = await _organizationService.GetOrganizationNameByUserIdAsync(user.UserId),
                Status = user.Status
            };

            return userResponse;
        }
    }
}
