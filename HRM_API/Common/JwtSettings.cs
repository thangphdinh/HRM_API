namespace HRM_API.Common
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public int AccessTokenExpireMinutes { get; set; }
        public int RefreshTokenExpireDays { get; set; }
    }
}
