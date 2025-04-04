using HRM_API.Common;
using HRM_API.Models.Requests;
using HRM_API.Models.Responses;

namespace HRM_API.Services
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> Login(LoginRequest request);
        Task<Result<LoginResponse>> RefreshToken(string refreshToken);
        Task<Result<bool>> Logout(string refreshToken);
    }
}
