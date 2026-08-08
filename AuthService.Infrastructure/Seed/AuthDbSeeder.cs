using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Seed
{
    public static class AuthDbSeeder
    {
        public static async Task SeedAdminAsync(AuthDbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();

            const string adminEmail = "admin@ecomm.com";

            var exists = await dbContext.Users.AnyAsync(x => x.Email == adminEmail);
            if (exists)
                return;

            var admin = new AppUser
            {
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.Users.AddAsync(admin);
            await dbContext.SaveChangesAsync();
        }
    }
}
