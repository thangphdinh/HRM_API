using System.ComponentModel.DataAnnotations;

namespace HRM_API.Models.Requests
{
    public class CreateUserRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int OrganizationId { get; set; }
    }
}
