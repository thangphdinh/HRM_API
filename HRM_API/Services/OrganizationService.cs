using HRM_API.Data;
using HRM_API.Models.Entities;
using HRM_API.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly AppDbContext _context;

        public OrganizationService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<int> GetOrganizationIdByUserIdAsync(int userId)
        {
            var user = await _context.Users.Where(u => u.UserId == userId)
                                           .Select(u => u.OrganizationId)
                                           .FirstOrDefaultAsync();

            return user;
        }

        public async Task<string> GetOrganizationNameByUserIdAsync(int userId)
        {
            var organizationId = await GetOrganizationIdByUserIdAsync(userId);

            var organizationName = await _context.Organizations
                .Where(o => o.OrganizationId == organizationId)
                .Select(o => o.OrganizationName)
                .FirstOrDefaultAsync();

            return organizationName;
        }

        public async Task<List<UserResponse>> GetUsersByOrganizationAsync(int organizationId)
        {
            var users = await _context.Users
                .Where(u => u.OrganizationId == organizationId)
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .ToListAsync();
            var userResponses = users.Select(u => new UserResponse
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.RoleName,
                Organization = u.Organization.OrganizationName,
                Status = u.Status
            }).ToList();
            return userResponses;
        }
    }
}
