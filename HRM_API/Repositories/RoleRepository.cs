using HRM_API.Common;
using HRM_API.Data;
using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleRepository> _logger;
        public RoleRepository(AppDbContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Result<Role>> GetRoleByIdAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(u => u.RoleId == roleId);

                // Nếu không tìm thấy role, trả về kết quả thất bại
                if (role == null)
                {
                    _logger.LogWarning($"Role with ID {roleId} not found.");
                    return Result<Role>.FailureResult("Role not found.");
                }

                // Nếu tìm thấy role, trả về kết quả thành công với dữ liệu role
                return Result<Role>.SuccessResult(role);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi trong quá trình truy vấn, trả về kết quả thất bại
                _logger.LogError(ex, "Error fetching role by ID.");
                return Result<Role>.FailureResult("Error fetching role: " + ex.Message);
            }
        }
    }
}
