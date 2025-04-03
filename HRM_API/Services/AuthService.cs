using HRM_API.Models.Entities;
using HRM_API.Models.Requests;
using HRM_API.Models.Responses;
using HRM_API.Repositories;
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

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            // Kiểm tra xem user có tồn tại không
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Tạo JWT AccessToken
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Lưu RefreshToken vào database
            _refreshTokenRepository.SaveRefreshToken(user.UserId, refreshToken);

            // Trả về AccessToken & RefreshToken
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<bool> Logout(string refreshToken)
        {
            var storedRefreshToken = _refreshTokenRepository.GetRefreshTokenByToken(refreshToken);
            if (storedRefreshToken == null)
            {
                return false;
            }
            await _refreshTokenRepository.DeleteRefreshToken(storedRefreshToken.UserId, refreshToken);
            return true;
        }

        public async Task<LoginResponse> RefreshToken(string refreshToken)
        {
            var storedRefreshToken = _refreshTokenRepository.GetRefreshTokenByToken(refreshToken);
            if (storedRefreshToken == null || storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return null;
            }

            var user = await _userRepository.GetUserByIdAsync(storedRefreshToken.UserId);
            if (user == null)
            {
                return null;
            }

            // Tạo access token mới
            var accessToken = GenerateAccessToken(user);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenerateAccessToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:AccessTokenExpireMinutes"])),
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
