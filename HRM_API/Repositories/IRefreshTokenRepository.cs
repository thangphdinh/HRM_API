using HRM_API.Models.Entities;

namespace HRM_API.Repositories
{
    public interface IRefreshTokenRepository
    {
        void SaveRefreshToken(int userId, string refreshToken);
        RefreshToken GetRefreshTokenByToken(string refreshToken);
        Task DeleteRefreshToken (int userId, string refreshToken);
    }
}
