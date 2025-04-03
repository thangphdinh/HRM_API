using HRM_API.Data;
using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task DeleteRefreshToken(int userId, string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken);
            if (token != null)
            {
                _context.RefreshTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Refresh token not found.");
            }
        }

        public RefreshToken GetRefreshTokenByToken(string refreshToken)
        {
            var token = _context.RefreshTokens
                                 .AsNoTracking()
                                 .FirstOrDefault(rt => rt.Token == refreshToken);
            if (token == null) return null;
            return token;
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
