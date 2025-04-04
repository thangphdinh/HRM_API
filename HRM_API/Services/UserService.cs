using HRM_API.Common;
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
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IOrganizationRepository organizationRepository, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public async Task<Result<List<UserResponse>>> GetAllUsersAsync()
        {
            try
            {
                // Gọi repository để lấy danh sách người dùng
                var usersResult = await _userRepository.GetAllUsersAsync();

                // Kiểm tra kết quả trả về từ repository
                if (!usersResult.Success)
                {
                    // Nếu không thành công, trả về kết quả thất bại
                    _logger.LogWarning($"Failed to fetch users: {usersResult.ErrorMessage}");
                    return Result<List<UserResponse>>.FailureResult(usersResult.ErrorMessage, usersResult.ErrorCode);
                }

                // Dựng danh sách các UserResponse từ dữ liệu người dùng
                var userResponses = usersResult.Data.Select(u => new UserResponse
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role.RoleName,
                    Organization = u.Organization.OrganizationName,
                    Status = u.Status
                }).ToList();

                // Trả về kết quả thành công với danh sách UserResponse
                return Result<List<UserResponse>>.SuccessResult(userResponses);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra trong quá trình xử lý, trả về kết quả thất bại
                _logger.LogError(ex, "Error fetching users");
                return Result<List<UserResponse>>.FailureResult("Error fetching users: " + ex.Message);
            }
        }

        public async Task<Result<UserResponse>> GetCurrentUserAsync()
        {
            try
            {
                // Lấy email từ claim của người dùng hiện tại
                var userEmail = _httpContextAccessor.HttpContext.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                // Nếu không có email trong claim thì trả về kết quả thất bại
                if (userEmail == null)
                {
                    _logger.LogWarning("User email not found in claims.");
                    return Result<UserResponse>.FailureResult("User email not found in claims.");
                }

                // Lấy thông tin người dùng từ repository
                var user = await _userRepository.GetUserByEmailAsync(userEmail);

                // Nếu không tìm thấy người dùng, trả về kết quả thất bại
                if (user == null)
                {
                    _logger.LogWarning($"User with email {userEmail} not found.");
                    return Result<UserResponse>.FailureResult("User not found.");
                }

                // Dựng UserResponse từ dữ liệu người dùng
                var userResponse = new UserResponse
                {
                    UserId = user.Data.UserId,
                    Username = user.Data.Username,
                    Email = user.Data.Email,
                    Role = user.Data.Role.RoleName,
                    Organization = user.Data.Organization.OrganizationName,
                    Status = user.Data.Status
                };

                // Trả về kết quả thành công với dữ liệu UserResponse
                return Result<UserResponse>.SuccessResult(userResponse);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra trong quá trình xử lý, trả về kết quả thất bại
                return Result<UserResponse>.FailureResult("Error fetching current user: " + ex.Message);
            }
        }
    }
}
