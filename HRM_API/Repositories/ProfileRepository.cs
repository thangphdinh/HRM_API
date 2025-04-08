using HRM_API.Common;
using HRM_API.Data;
using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProfileRepository> _logger;

        public ProfileRepository(AppDbContext context, ILogger<ProfileRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Result<Profile>> GetProfileByUserIdAsync(int userId)
        {
            try
            {
                var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile == null)
                {
                    _logger.LogWarning($"Profile not found for userId: {userId}");
                    return Result<Profile>.FailureResult("Profile not found", 404);
                }
                return Result<Profile>.SuccessResult(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting profile for userId: {userId}");
                return Result<Profile>.FailureResult("An error occurred while retrieving the profile", 500);
            }
        }
    }
}
