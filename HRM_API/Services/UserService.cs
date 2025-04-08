using HRM_API.Common;
using HRM_API.Data;
using HRM_API.Models.Entities;
using HRM_API.Models.Requests;
using HRM_API.Models.Responses;
using HRM_API.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRM_API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IOrganizationRepository organizationRepository, IRoleRepository roleRepository, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger, IPasswordHasher passwordHasher, IProfileRepository profileRepository)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _profileRepository = profileRepository;
        }

        public async Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request)
        {
            // Kiểm tra các thông tin yêu cầu không được để trống
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return Result<UserResponse>.FailureResult("Username, Email and Password are required.");
            }

            try
            {
                //Kiểm tra xem người dùng đã tồn tại hay chưa
                var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existingUser.Success && existingUser.Data != null)
                {
                    return Result<UserResponse>.FailureResult("User with this email already exists.");
                }

                // Kiểm tra xem RoleId và OrganizationId có hợp lệ không
                var role = await _roleRepository.GetRoleByIdAsync(request.RoleId);
                var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
                if (!role.Success)
                {
                    return Result<UserResponse>.FailureResult("Role not found.");
                }
                if (!organization.Success)
                {
                    return Result<UserResponse>.FailureResult("Organization not found.");
                }

                //Mã hoá dữ liệu
                var passwordHash = _passwordHasher.HashPassword(request.Password);

                //Claim userId từ HttpContext
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Result<UserResponse>.FailureResult("User ID not found in claims.");
                }
                var currenUserId = int.Parse(userIdClaim);

                //Tạo người dùng mới
                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    RoleId = request.RoleId,
                    OrganizationId = request.OrganizationId,
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = currenUserId,
                };

                // Lưu người dùng mới vào cơ sở dữ liệu
                var result = await _userRepository.CreateUserAsync(newUser);

                // Kiểm tra kết quả trả về từ repository
                if (result.Success)
                {
                    // Nếu thành công, dựng UserResponse từ dữ liệu người dùng mới
                    var userResponse = new UserResponse
                    {
                        UserId = result.Data.UserId,
                        Username = result.Data.Username,
                        Email = result.Data.Email,
                        Role = role.Data.RoleName,
                        Organization = organization.Data.OrganizationName,
                        Status = result.Data.Status
                    };
                    return Result<UserResponse>.SuccessResult(userResponse);
                }
                else
                {
                    // Nếu không thành công, trả về kết quả thất bại
                    _logger.LogWarning($"Failed to create user: {result.ErrorMessage}");
                    return Result<UserResponse>.FailureResult(result.ErrorMessage, result.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra trong quá trình xử lý, trả về kết quả thất bại
                _logger.LogError(ex, "Error creating user");
                return Result<UserResponse>.FailureResult("Error creating user: " + ex.Message);
            }
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
                var userEmail = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

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

        public async Task<Result<UserDetailResponse>> GetUserDetailAsync(int userId)
        {
            try
            {
                var userResult = await _userRepository.GetUserByIdAsync(userId);
                if (!userResult.Success)
                {
                    _logger.LogWarning($"Failed to fetch user: {userResult.ErrorMessage}");
                    return Result<UserDetailResponse>.FailureResult(userResult.ErrorMessage, userResult.ErrorCode);
                }
                var user = userResult.Data;

                var profileResult = await _profileRepository.GetProfileByUserIdAsync(userId);
                var profile = profileResult.Success ? profileResult.Data : null;

                var organizationResult = await _organizationRepository.GetOrganizationByIdAsync(user.OrganizationId);
                var organization = organizationResult.Success ? organizationResult.Data : null;

                var response = new UserDetailResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.RoleName,
                    Organization = user.Organization.OrganizationName,
                    Status = user.Status,
                    Profile = profile != null ? new ProfileResponse
                    {
                        FirstName = profile.FirstName,
                        MiddleName = profile.MiddleName,
                        LastName = profile.LastName,
                        BirthDate = profile.BirthDate,
                        Gender = profile.Gender,
                        AvatarUrl = profile.AvatarUrl,
                        PhoneNumber = profile.PhoneNumber,
                        EmailProfile = profile.EmailProfile,
                        Address = profile.Address,
                        Nationality = profile.Nationality,
                        IdentityNumber = profile.IdentityNumber,
                        TaxCode = profile.TaxCode,
                        MaritalStatus = profile.MaritalStatus,
                        Position = profile.Position,
                        Department = profile.Department,
                        JoinDate = profile.JoinDate,
                        ResignDate = profile.ResignDate
                    } : null
                };
                return Result<UserDetailResponse>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user detail");
                return Result<UserDetailResponse>.FailureResult("Error fetching user detail: " + ex.Message);
            }
        }
    }
}
