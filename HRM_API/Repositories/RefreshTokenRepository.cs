using HRM_API.Data;
using HRM_API.Models.Entities;

namespace HRM_API.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public RefreshToken GetRefreshTokenByToken(string refreshToken)
        {
            return _context.RefreshTokens
            .FirstOrDefault(rt => rt.Token == refreshToken);
        }

        public void SaveRefreshToken(int userId, string refreshToken)
        {
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDate = DateTime.Now.AddMonths(1) // RefreshToken hết hạn sau 1 tháng
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            _context.SaveChanges();
        }
    }
}
