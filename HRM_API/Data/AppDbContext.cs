using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Disable cascade delete mặc định
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Cấu hình User
            modelBuilder.Entity<User>(e =>
            {
                e.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(u => u.Organization)
                    .WithMany(o => o.Users)
                    .HasForeignKey(u => u.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(u => u.CreatedByUser)
                    .WithMany(u => u.CreatedUsers)
                    .HasForeignKey(u => u.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình Role
            modelBuilder.Entity<Role>(e =>
            {
                e.HasOne(r => r.Organization)
                    .WithMany(o => o.Roles)
                    .HasForeignKey(r => r.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Data (Chỉ dùng cho development)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Sử dụng giá trị cố định cho DateTime để tránh lỗi migration
            var currentDate = new DateTime(2025, 4, 3); // Giá trị cố định

            // System Organization
            var systemOrg = new Organization
            {
                OrganizationId = 1,
                OrganizationName = "SYSTEM",
                LicenseKey = "ROOT_KEY",
            };

            modelBuilder.Entity<Organization>().HasData(systemOrg);

            // SystemAdmin Role
            modelBuilder.Entity<Role>().HasData(new Role
            {
                RoleId = 1,
                RoleName = "SystemAdmin",
                OrganizationId = systemOrg.OrganizationId
            });

            // SystemAdmin User
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "sysadmin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Thang@123"),
                Email = "sysadmin@hrm.com",
                RoleId = 1,
                OrganizationId = systemOrg.OrganizationId,
                CreatedBy = null,
                CreatedAt = currentDate, // Sử dụng giá trị cố định cho CreatedAt
                UpdatedAt = currentDate  // Sử dụng giá trị cố định cho UpdatedAt
            });
        }
    }
}
