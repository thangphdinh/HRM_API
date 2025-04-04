using HRM_API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            // Disable cascade delete mặc định cho tất cả các mối quan hệ
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
                    .OnDelete(DeleteBehavior.Restrict);  // Không cascade khi xóa

                e.HasOne(u => u.Organization)
                    .WithMany(o => o.Users)
                    .HasForeignKey(u => u.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);  // Không cascade khi xóa

                e.HasOne(u => u.CreatedByUser)
                    .WithMany(u => u.CreatedUsers)
                    .HasForeignKey(u => u.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);  // Đảm bảo không cascade cho self-referencing relationship

                // Đảm bảo email là duy nhất
                e.HasIndex(u => u.Email).IsUnique();
            });

            // Cấu hình Role
            modelBuilder.Entity<Role>(e =>
            {
                // Đảm bảo RoleName là duy nhất trong cơ sở dữ liệu
                e.HasIndex(r => r.RoleName).IsUnique();
            });

            // Cấu hình Organization
            modelBuilder.Entity<Organization>(e =>
            {
                // Đảm bảo LicenseKey là duy nhất trong cơ sở dữ liệu
                e.HasIndex(o => o.LicenseKey).IsUnique();
            });

            // Seed Data (Chỉ dùng cho development)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var currentDate = new DateTime(2025, 4, 3);

            var systemOrg = new Organization
            {
                OrganizationId = 1,
                OrganizationName = "SYSTEM",
                LicenseKey = "ROOT_KEY",
                CreatedAt = currentDate,
                UpdatedAt = currentDate
            };

            var sysadminPasswordHash = "$2b$12$46nJBfmAtQzAMngec9IHEOaxl/WGYr09sqD6oNXAZb.dZqemc7LHa"; // hash cố định từ "Thang@123"

            // Thêm hệ thống tổ chức vào DB
            modelBuilder.Entity<Organization>().HasData(systemOrg);

            // Thêm role SystemAdmin vào DB
            modelBuilder.Entity<Role>().HasData(new Role
            {
                RoleId = 1,
                RoleName = "SystemAdmin"
            });

            // Thêm user Sysadmin vào DB
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "sysadmin",
                PasswordHash = sysadminPasswordHash,
                Email = "sysadmin@hrm.com",
                RoleId = 1,
                OrganizationId = systemOrg.OrganizationId,
                CreatedBy = null,
                CreatedAt = currentDate,
                UpdatedAt = currentDate
            });
        }
    }
}
