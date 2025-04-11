using HRM_API.Common;
using HRM_API.Models.Responses;
using HRM_API.Repositories;

namespace HRM_API.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleService> _logger;
        public RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<Result<List<RoleResponse>>> GetAllRolesAsync()
        {
            try
            {
                // Gọi repository
                var rolesResult = await _roleRepository.GetAllRolesAsync();

                // Kiểm tra kết quả trả về từ repository
                if (!rolesResult.Success)
                {
                    // Nếu không thành công, trả về kết quả thất bại
                    _logger.LogWarning($"Failed to fetch users: {rolesResult.ErrorMessage}");
                    return Result<List<RoleResponse>>.FailureResult(rolesResult.ErrorMessage, rolesResult.ErrorCode);
                }

                // Dựng danh sách các UserResponse từ dữ liệu người dùng
                var roleResponses = rolesResult.Data.Select(u => new RoleResponse
                {
                    RoleId = u.RoleId,
                    RoleName = u.RoleName
                }).ToList();

                // Trả về kết quả thành công với danh sách UserResponse
                return Result<List<RoleResponse>>.SuccessResult(roleResponses);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra trong quá trình xử lý, trả về kết quả thất bại
                _logger.LogError(ex, "Error fetching users");
                return Result<List<RoleResponse>>.FailureResult("Error fetching users: " + ex.Message);
            }
        }
    }
}
