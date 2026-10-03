using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Models;

namespace Restaurant.Api.Data
{
    public class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext db, IConfiguration config)
        {
            await db.Database.MigrateAsync();

            if (await db.Users.AnyAsync(u => u.Role == Role.Admin))
                return;

            var admin = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = config["Seed:AdminEmail"]!,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(config["Seed:AdminPassword"]!),
                Role = Role.Admin
            };

            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}
