using HRM_API.Common;
using HRM_API.Models.Entities;

namespace HRM_API.Repositories
{
    public interface IRoleRepository
    {
        Task<Result<Role>> GetRoleByIdAsync(int roleId);
        Task<Result<List<Role>>> GetAllRolesAsync();
    }
}
