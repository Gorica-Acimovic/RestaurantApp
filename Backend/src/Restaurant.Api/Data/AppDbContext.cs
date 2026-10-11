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
            public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
            public DbSet<MenuItem> MenuItems => Set<MenuItem>();
            public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<MenuSubcategory> MenuSubcategories => Set<MenuSubcategory>();

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


            modelBuilder.Entity<RestaurantTable>(e =>
            {
                e.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(t => t.Number).IsUnique().HasFilter("[IsActive]=1");
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_RestaurantTables_Number", "[Number] >= 1");
                    t.HasCheckConstraint("CK_RestaurantTables_Capacity", "[Capacity] >= 1");
                    t.HasCheckConstraint("CK_RestaurantTables_PositionX", "[PositionX] >= 0 AND [PositionX] <= 100");
                    t.HasCheckConstraint("CK_RestaurantTables_PositionY", "[PositionY] >= 0 AND [PositionY] <= 100");

                });

            });

            modelBuilder.Entity<MenuItem>(e =>
            {
                e.Property(m => m.Name).HasMaxLength(100);
                e.Property(m => m.Description).HasMaxLength(500);
                e.Property(m => m.Price).HasPrecision(10, 2);
                e.HasOne(m => m.Subcategory)
                    .WithMany()
                    .HasForeignKey(m => m.SubcategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_MenuItems_Price", "[Price] >= 0");
                });
            });


            modelBuilder.Entity<Reservation>(e =>
            {
                e.Property(r => r.CustomerName).IsRequired().HasMaxLength(100);
                e.Property(r => r.CustomerEmail).HasMaxLength(200);
                e.Property(r => r.CustomerPhone).HasMaxLength(20);
                e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
                e.Property(r => r.RowVersion).IsRowVersion();

                e.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(e =>e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Table)
                    .WithMany()
                    .HasForeignKey(e => e.TableId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(r => new {r.TableId, r.StartsAt})
                    .IsUnique()
                    .HasFilter("[TableId] IS NOT NULL AND [Status] IN ('Pending', 'Confirmed')");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Reservations_NumberOfGuests", "[NumberOfGuests] >= 1");
                    t.HasCheckConstraint("CK_Reservations_StartsAt", "[StartsAt] < [EndsAt]");
                });


            });

            modelBuilder.Entity<MenuSubcategory>(e =>
            {
                e.Property(s => s.Name).IsRequired().HasMaxLength(50);
                e.Property(s => s.Category).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(s => new { s.Category, s.Name }).IsUnique();

                e.HasData(
                    new MenuSubcategory { Id = 1, Name = "Appetizers", Category = MenuCategory.Food, SortOrder = 1 },
                    new MenuSubcategory { Id = 2, Name = "Salads", Category = MenuCategory.Food, SortOrder = 2 },
                    new MenuSubcategory { Id = 3, Name = "Main courses", Category = MenuCategory.Food, SortOrder = 3 },
                    new MenuSubcategory { Id = 4, Name = "Desserts", Category = MenuCategory.Food, SortOrder = 4 },
                    new MenuSubcategory { Id = 5, Name = "Coffee & tea", Category = MenuCategory.Drink, SortOrder = 1 },
                    new MenuSubcategory { Id = 6, Name = "Soft drinks", Category = MenuCategory.Drink, SortOrder = 2 },
                    new MenuSubcategory { Id = 7, Name = "Beer", Category = MenuCategory.Drink, SortOrder = 3 },
                    new MenuSubcategory { Id = 8, Name = "Wine", Category = MenuCategory.Drink, SortOrder = 4 },
                    new MenuSubcategory { Id = 9, Name = "Cocktails", Category = MenuCategory.Drink, SortOrder = 5 },
                    new MenuSubcategory { Id = 10, Name = "Spirits", Category = MenuCategory.Drink, SortOrder = 6 }
                );
            });




        }

       

    } 
    
}

    

   
   
