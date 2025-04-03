using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Models.Entities
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } // Navigation property

        [Required]
        [StringLength(255)]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }
    }
}
