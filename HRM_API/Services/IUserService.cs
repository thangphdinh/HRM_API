using HRM_API.Models.Entities;
using HRM_API.Models.Responses;

namespace HRM_API.Services
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<UserResponse> GetCurrentUserAsync();
    }
}
