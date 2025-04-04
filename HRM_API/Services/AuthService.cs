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
        private readonly IConfiguration _configuration;
        private readonly IOptions<JwtSettings> _jwtSettings;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _jwtSettings = jwtSettings;
        }
        public async Task<Result<LoginResponse>> Login(LoginRequest request)
        {
            // Kiểm tra xem user có tồn tại không
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Data.PasswordHash))
            {
               return Result<LoginResponse>.FailureResult("Invalid email or password");
            }
            // Tạo JWT AccessToken
            var accessToken = GenerateAccessToken(user.Data);
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

        private string GenerateAccessToken(User user)
        {
            var secretKey = _jwtSettings.Value.SecretKey;
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.AccessTokenExpireMinutes),
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
