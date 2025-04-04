using HRM_API.Common;
using HRM_API.Models.Entities;

namespace HRM_API.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<Result<bool>> SaveRefreshTokenAsync(int userId, string refreshToken);
        Task<Result<RefreshToken>> GetRefreshTokenByTokenAsync(string refreshToken);
        Task<Result<bool>> DeleteRefreshTokenAsync(int userId, string refreshToken);
    }
}
