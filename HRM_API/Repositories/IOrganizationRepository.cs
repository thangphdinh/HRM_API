using HRM_API.Common;
using HRM_API.Models.Entities;

namespace HRM_API.Repositories
{
    public interface IOrganizationRepository
    {
        Task<Result<int?>> GetOrganizationIdByUserIdAsync(int userId);
        Task<Result<Organization>> GetOrganizationByIdAsync(int organizationId);
        Task<Result<List<User>>> GetUsersByOrganizationIdAsync(int organizationId);
    }
}
