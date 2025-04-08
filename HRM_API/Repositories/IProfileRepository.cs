using HRM_API.Common;
using HRM_API.Models.Entities;

namespace HRM_API.Repositories
{
    public interface IProfileRepository
    {
        Task<Result<Profile>> GetProfileByUserIdAsync(int userId);
    }
}
