namespace HRM_API.Models.Responses
{
    public class RegisterResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}
