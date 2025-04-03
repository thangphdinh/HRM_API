namespace HRM_API.Models.Responses
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Organization { get; set; }
        public bool Status { get; set; }
    }
}
