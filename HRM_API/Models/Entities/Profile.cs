﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Models.Entities
{
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [NotMapped]
        public string FullName => $"{LastName} {MiddleName} {FirstName}".Trim();

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [StringLength(255)]
        public string? AvatarUrl { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        public string? EmailProfile { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        [StringLength(50)]
        public string? IdentityNumber { get; set; }

        [StringLength(50)]
        public string? TaxCode { get; set; }

        [StringLength(50)]
        public string? MaritalStatus { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public DateTime? JoinDate { get; set; }
        public DateTime? ResignDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
