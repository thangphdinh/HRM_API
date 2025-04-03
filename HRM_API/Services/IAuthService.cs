using HRM_API.Models.Requests;
using HRM_API.Models.Responses;

namespace HRM_API.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task<LoginResponse> RefreshToken(string refreshToken);
    }
}
