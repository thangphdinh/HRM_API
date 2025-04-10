using HRM_API.Common;
using HRM_API.Models.Entities;
using HRM_API.Models.Requests;
using HRM_API.Models.Responses;
using HRM_API.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRM_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IOptions<JwtSettings> jwtSettings, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtSettings = jwtSettings;
            _passwordHasher = passwordHasher;
        }
        public async Task<Result<LoginResponse>> Login(LoginRequest request)
        {
            // Kiểm tra xem user có tồn tại không
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (!user.Success || user.Data == null)
            {
                return Result<LoginResponse>.FailureResult("Invalid email or password");
            }
            if (user.Data.Status == false)
            {
                return Result<LoginResponse>.FailureResult("User is not active");
            }
            if (!_passwordHasher.VerifyPassword(user.Data.PasswordHash, request.Password))
            {
                return Result<LoginResponse>.FailureResult("Invalid email or password");
            }
            // Kiểm tra thời gian hết hạn của token có rememberMe
            var expireMinutes = request.RememberMe ? 60 * 24 : (int?)null;
            // Tạo JWT AccessToken
            var accessToken = GenerateAccessToken(user.Data, expireMinutes);
            var refreshToken = GenerateRefreshToken();
            // Lưu RefreshToken vào database
            await _refreshTokenRepository.SaveRefreshTokenAsync(user.Data.UserId, refreshToken);
            // Trả về AccessToken & RefreshToken
            return Result<LoginResponse>.SuccessResult(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        public async Task<Result<bool>> Logout(string refreshToken)
        {
            var storedRefreshToken = await _refreshTokenRepository.GetRefreshTokenByTokenAsync(refreshToken);
            // Kiểm tra kết quả trả về từ repository (thành công và dữ liệu không null)
            if (!storedRefreshToken.Success || storedRefreshToken.Data == null)
            {
                return Result<bool>.FailureResult("Refresh token not found or invalid.");
            }
            // Xóa RefreshToken khỏi database
            await _refreshTokenRepository.DeleteRefreshTokenAsync(storedRefreshToken.Data.UserId, refreshToken);
            return Result<bool>.SuccessResult(true);
        }

        public async Task<Result<LoginResponse>> RefreshToken(string refreshToken)
        {
            var storedRefreshToken = await _refreshTokenRepository.GetRefreshTokenByTokenAsync(refreshToken);
            if (storedRefreshToken.Data == null || storedRefreshToken.Data.ExpiryDate < DateTime.UtcNow)
            {
                return Result<LoginResponse>.FailureResult("Refresh token NOT FOUND or EXP");
            }
            var user = await _userRepository.GetUserByIdAsync(storedRefreshToken.Data.UserId);
            if (user == null)
            {
                return Result<LoginResponse>.FailureResult("User not found."); ;
            }
            // Tạo access token mới
            var accessToken = GenerateAccessToken(user.Data);
            return Result< LoginResponse>.SuccessResult(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        private string GenerateAccessToken(User user, int? expireMinutes = null)
        {
            var secretKey = _jwtSettings.Value.SecretKey;
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var expires = DateTime.UtcNow.AddMinutes(expireMinutes ?? _jwtSettings.Value.AccessTokenExpireMinutes);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
