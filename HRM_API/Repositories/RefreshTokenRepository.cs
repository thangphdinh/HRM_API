using HRM_API.Common;
using HRM_API.Data;
using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HRM_API.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RefreshTokenRepository> _logger;
        private readonly IOptions<JwtSettings> _jwtSettings;

        public RefreshTokenRepository(AppDbContext context, ILogger<RefreshTokenRepository> logger, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _logger = logger;
            _jwtSettings = jwtSettings;
        }

        public async Task<Result<bool>> DeleteRefreshTokenAsync(int userId, string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken);
                if (token != null)
                {
                    _context.RefreshTokens.Remove(token);
                    await _context.SaveChangesAsync();

                    // Trả về Result thành công
                    return Result<bool>.SuccessResult(true);
                }
                else
                {
                    // Nếu không tìm thấy token, trả về Result thất bại
                    _logger.LogWarning($"Refresh token not found.");
                    return Result<bool>.FailureResult("Refresh token not found.");
                }
            }
            catch (Exception ex)
            {
                // Trả về Result thất bại với thông báo lỗi
                _logger.LogError(ex, "Error deleting refresh token");
                return Result<bool>.FailureResult("Error deleting refresh token: " + ex.Message);
            }
        }

        public async Task<Result<RefreshToken>> GetRefreshTokenByTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                     .AsNoTracking()
                     .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
                if (token == null)
                {
                    _logger.LogWarning($"Refresh token {refreshToken} not found.");
                    return Result<RefreshToken>.FailureResult($"Refresh token {refreshToken} not found.");
                }
                return Result<RefreshToken>.SuccessResult(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refresh token");
                return Result<RefreshToken>.FailureResult("Error retrieving refresh token");
            }

        }


        public async Task<Result<bool>> SaveRefreshTokenAsync(int userId, string refreshToken)
        {
            try
            {
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken,
                    ExpiryDate = DateTime.Now.AddDays(_jwtSettings.Value.RefreshTokenExpireDays)
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                return Result<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token");
                return Result<bool>.FailureResult("Error saving refresh token: " + ex.Message);
            }
        }
    }
}
