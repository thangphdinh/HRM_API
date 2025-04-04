using HRM_API.Common;
using HRM_API.Data;
using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrganizationRepository> _logger;
        public OrganizationRepository(AppDbContext context, ILogger<OrganizationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Result<Organization>> GetOrganizationByIdAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.OrganizationId == organizationId);

                if (organization == null)
                {
                    _logger.LogWarning($"Organization with ID {organizationId} not found.");
                    return Result<Organization>.FailureResult("Organization not found.");
                }

                return Result<Organization>.SuccessResult(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization");
                return Result<Organization>.FailureResult("An error occurred while retrieving organization: " + ex.Message);
            }
        }

        public async Task<Result<int?>> GetOrganizationIdByUserIdAsync(int userId)
        {
            try
            {
                var organizationId = await _context.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => (int?)u.OrganizationId)
                    .FirstOrDefaultAsync();

                if (organizationId == null)
                {
                    _logger.LogWarning($"Organization ID not found for user with ID {userId}.");
                    return Result<int?>.FailureResult("Organization ID not found for the user.");

                }

                return Result<int?>.SuccessResult(organizationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization ID");
                return Result<int?>.FailureResult("An error occurred while retrieving organization ID: " + ex.Message);
            }
        }

        public async Task<Result<List<User>>> GetUsersByOrganizationIdAsync(int organizationId)
        {
            var users = await _context.Users
                    .Where(u => u.OrganizationId == organizationId)
                    .Include(u => u.Role)
                    .Include(u => u.Organization)
                    .ToListAsync();

            if (!users.Any())
            {
                _logger.LogWarning($"No users found for organization with ID {organizationId}.");
                return Result<List<User>>.FailureResult("No users found for the organization.");
            }

            return Result<List<User>>.SuccessResult(users);
        }
    }
}
