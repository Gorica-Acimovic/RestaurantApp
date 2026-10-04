using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Abstractions;
using Restaurant.Api.Models;

namespace Restaurant.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
            public DbSet<User> Users => Set<User>();
            public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.PinHash).HasMaxLength(64);
                e.HasIndex(u => u.PinHash).IsUnique().HasFilter("[PinHash] is not null and [IsActive]=1");
                e.Property(u => u.Email).HasMaxLength(200);
                e.Property(u => u.FirstName).HasMaxLength(100);
                e.Property(u => u.LastName).HasMaxLength(100);
                e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                e.HasOne(u => u.RegisteredBy)
                    .WithMany()
                    .HasForeignKey(u => u.RegisteredById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.Property(t => t.Token).HasMaxLength(200);
                e.HasIndex(t => t.Token).IsUnique();
            });
        }

       

    } 
    
}

    

   
   
