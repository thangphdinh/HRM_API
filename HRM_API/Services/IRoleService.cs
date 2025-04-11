using HRM_API.Common;
using HRM_API.Models.Responses;

namespace HRM_API.Services
{
    public interface IRoleService
    {
        Task<Result<List<RoleResponse>>> GetAllRolesAsync();
    }
}
