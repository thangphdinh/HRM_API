using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM_API.Models.Entities
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }

        // Foreign Key
        public int OrganizationId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization Organization { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
