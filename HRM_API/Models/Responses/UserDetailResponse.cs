namespace HRM_API.Models.Responses
{
    public class UserDetailResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Organization { get; set; }
        public bool Status { get; set; }
        public ProfileResponse? Profile { get; set; }
    }

    public class ProfileResponse
    {
        // Profile Info
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{LastName} {MiddleName} {FirstName}".Trim();
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string? EmailProfile { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public string? IdentityNumber { get; set; }
        public string? TaxCode { get; set; }
        public string? MaritalStatus { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ResignDate { get; set; }
    }
}
