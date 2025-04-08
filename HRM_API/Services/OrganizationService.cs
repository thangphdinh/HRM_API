using HRM_API.Common;
using HRM_API.Data;
using HRM_API.Models.Entities;
using HRM_API.Models.Responses;
using HRM_API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(IOrganizationRepository organizationRepository, ILogger<OrganizationService> logger)
        {
            _organizationRepository = organizationRepository;
            _logger = logger;
        }

        public async Task<Result<OrganizationResponse>> GetOrganizationInforByUserIdAsync(int userId)
        {
            // Lấy organizationId từ userId
            var orgIdResult = await _organizationRepository.GetOrganizationIdByUserIdAsync(userId);
            if (!orgIdResult.Success || orgIdResult.Data == null)
            {
                // Nếu không tìm thấy organizationId, trả về kết quả thất bại
                _logger.LogWarning($"OrganizationId not found for UserId {userId}: {orgIdResult.ErrorMessage}");
                return Result<OrganizationResponse>.FailureResult(orgIdResult.ErrorMessage);
            }

            // Lấy organization details từ id
            var orgResult = await _organizationRepository.GetOrganizationByIdAsync(orgIdResult.Data.Value);
            if (!orgResult.Success || orgResult.Data == null)
            {
                // Nếu không tìm thấy organization, trả về kết quả thất bại
                _logger.LogWarning($"Organization not found for OrganizationId {orgIdResult.Data.Value}: {orgResult.ErrorMessage}");
                return Result<OrganizationResponse>.FailureResult(orgResult.ErrorMessage);
            }

            // Tạo response
            var response = new OrganizationResponse
            {
                OrganizationId = orgResult.Data.OrganizationId,
                OrganizationName = orgResult.Data.OrganizationName
            };

            return Result<OrganizationResponse>.SuccessResult(response);
        }

        public async Task<Result<List<UserResponse>>> GetUsersByOrganizationAsync(int organizationId)
        {
            var usersResult = await _organizationRepository.GetUsersByOrganizationIdAsync(organizationId);

            if (!usersResult.Success)
                return Result<List<UserResponse>>.FailureResult(usersResult.ErrorMessage);

            var userResponses = usersResult.Data.Select(u => new UserResponse
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.RoleName,
                Organization = u.Organization.OrganizationName,
                Status = u.Status
            }).ToList();

            return Result<List<UserResponse>>.SuccessResult(userResponses);
        }
    }
}
