using HRM_API.Data;
using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.Include(u => u.Role)
                                  .Include(u => u.Organization)
                                  .ToListAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                                    .Include(u => u.Role)
                                    .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return null;
            return user;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.Status);
            if (user == null) return null;
            return user;
        }
    }
}
