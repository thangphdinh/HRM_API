using HRM_API.Common;
using HRM_API.Data;
using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<User>> CreateUserAsync(User user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Nếu thêm người dùng thành công, trả về kết quả thành công với dữ liệu User
                return Result<User>.SuccessResult(user);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi khi thêm người dùng, rollback transaction
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating user.");
                return Result<User>.FailureResult("Error creating user: " + ex.Message);
            }
        }

        public async Task<Result<List<User>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users.Include(u => u.Role)
                                                .Include(u => u.Organization)
                                                .ToListAsync();

                if (users != null && users.Any())
                {
                    // Nếu có người dùng, trả về kết quả thành công với dữ liệu
                    return Result<List<User>>.SuccessResult(users);
                }
                else
                {
                    // Nếu không có người dùng, trả về kết quả thất bại
                    _logger.LogWarning("No users found in the database.");
                    return Result<List<User>>.FailureResult("No users found.");
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi khi truy vấn, trả về kết quả thất bại với thông báo lỗi
                _logger.LogError(ex, "Error fetching users from the database.");
                return Result<List<User>>.FailureResult("Error fetching users: " + ex.Message);
            }
        }

        public async Task<Result<User>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                                    .Include(u => u.Role)
                                    .Include(u => u.Organization)
                                    .FirstOrDefaultAsync(u => u.Email == email);
                if (user !=null)
                {
                    // Nếu tìm thấy người dùng, trả về kết quả thành công với dữ liệu
                    return Result<User>.SuccessResult(user);
                }
                else
                {
                    // Nếu không tìm thấy người dùng, trả về kết quả thất bại
                    _logger.LogWarning($"User with email {email} not found.");
                    return Result<User>.FailureResult($"User with email {email} not found.");
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi khi truy vấn, trả về kết quả thất bại với thông báo lỗi
                _logger.LogError(ex, "Error fetching user by email.");
                return Result<User>.FailureResult("Error fetching user by email: " + ex.Message);
            }
        }

        public async Task<Result<User>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                                          .Include(u => u.Role)
                                          .FirstOrDefaultAsync(u => u.UserId == userId && u.Status);

                // Nếu không tìm thấy người dùng, trả về kết quả thất bại
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found or inactive.");
                    return Result<User>.FailureResult("User not found or inactive.");
                }

                // Nếu tìm thấy người dùng, trả về kết quả thành công với dữ liệu User
                return Result<User>.SuccessResult(user);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi trong quá trình truy vấn, trả về kết quả thất bại
                _logger.LogError(ex, "Error fetching user by ID.");
                return Result<User>.FailureResult("Error fetching user: " + ex.Message);
            }
        }
    }
}
