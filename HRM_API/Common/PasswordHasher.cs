using System.Security.Cryptography;
using System.Text;

namespace HRM_API.Common
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Sử dụng BCrypt để mã hóa mật khẩu
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            // Sử dụng BCrypt để xác thực mật khẩu
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
