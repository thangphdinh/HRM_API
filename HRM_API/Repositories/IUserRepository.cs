using HRM_API.Common;
using HRM_API.Models.Entities;
namespace HRM_API.Repositories
{
    public interface IUserRepository
    {
        Task<Result<User>> GetUserByEmailAsync(string email);
        Task<Result<User>> GetUserByIdAsync(int userId);
        Task<Result<List<User>>> GetAllUsersAsync();
    }
}

