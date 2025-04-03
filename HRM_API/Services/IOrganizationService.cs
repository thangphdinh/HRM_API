using HRM_API.Models.Entities;
using HRM_API.Models.Responses;

namespace HRM_API.Services
{
    public interface IOrganizationService
    {
        Task<int> GetOrganizationIdByUserIdAsync(int userId);
        Task<string> GetOrganizationNameByUserIdAsync(int userId);
        Task<List<UserResponse>> GetUsersByOrganizationAsync(int organizationId);
    }
}
