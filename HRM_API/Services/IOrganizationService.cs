using HRM_API.Common;
using HRM_API.Models.Entities;
using HRM_API.Models.Responses;

namespace HRM_API.Services
{
    public interface IOrganizationService
    {
        Task<Result<List<UserResponse>>> GetUsersByOrganizationAsync(int organizationId);
        Task<Result<OrganizationResponse>> GetOrganizationInforByUserIdAsync (int userId);
    }
}
