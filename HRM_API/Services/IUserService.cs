using HRM_API.Common;
using HRM_API.Models.Requests;
using HRM_API.Models.Responses;

namespace HRM_API.Services
{
    public interface IUserService
    {
        Task<Result<List<UserResponse>>> GetAllUsersAsync();
        Task<Result<UserResponse>> GetCurrentUserAsync();
        Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request);
        Task<Result<UserDetailResponse>> GetUserDetailAsync(int userId);
    }
}
