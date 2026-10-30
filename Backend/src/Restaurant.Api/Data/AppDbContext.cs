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
        
            public DbSet<Order> Orders => Set<Order>();
            public DbSet<OrderItem> OrderItems => Set<OrderItem>();
            public DbSet<Invoice> Invoices => Set<Invoice>();
            public DbSet<Review> Reviews => Set<Review>();

            public DbSet<DailyStatistic> DailyStatistics => Set<DailyStatistic>();
            public DbSet<Prediction> Predictions => Set<Prediction>();

            public DbSet<PinLoginAttempt> PinLoginAttempts => Set<PinLoginAttempt>();

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

            modelBuilder.HasSequence<int>("InvoiceNumbers").StartsAt(1).IncrementsBy(1);


            modelBuilder.Entity<Order>(e =>
            {
                e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

                e.HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(o => o.Waiter)
                    .WithMany()
                    .HasForeignKey(o => o.WaiterId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(o => o.TableId)
                    .IsUnique()
                    .HasFilter("[Status] = 'Open'");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Orders_NumberOfGuests", "[NumberOfGuests] >= 1");
                });

            });


            modelBuilder.Entity<OrderItem>(e => 
            {
                e.Property(i => i.UnitPrice).HasPrecision(10, 2);
                e.Property(i => i.Note).HasMaxLength(200);

                e.HasOne(i => i.Order)
                 .WithMany(o => o.Items)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(i => i.MenuItem)
                    .WithMany()
                    .HasForeignKey(i => i.MenuItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] >= 1");
                    t.HasCheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] >= 0");

                });

             });


            modelBuilder.Entity<Invoice>(e =>
            {
                e.Property(i => i.InvoiceNumber).HasDefaultValueSql("NEXT VALUE FOR InvoiceNumbers");
                e.Property(i => i.PaymentMethod).HasConversion<string>().HasMaxLength(20);
                e.Property(i => i.TotalAmount).HasPrecision(10, 2);

                e.HasIndex(i => i.InvoiceNumber).IsUnique();

                e.HasOne(i => i.Order)
                    .WithOne(o => o.Invoice)
                    .HasForeignKey<Invoice>(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Invoices_TotalAmount", "[TotalAmount] >= 0");
                });
            });


            modelBuilder.Entity<Review>(e =>
            {
                e.Property(r => r.Comment).HasMaxLength(1000);
                e.Property(r => r.ManagerResponse).HasMaxLength(1000);
                e.Property(r => r.RowVersion).IsRowVersion();

                e.HasOne(r => r.Reservation)
                    .WithOne(x => x.Review)
                    .HasForeignKey<Review>(r => r.ReservationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Responder)
                    .WithMany()
                    .HasForeignKey(r => r.ResponderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5");
                    t.HasCheckConstraint("CK_Reviews_Response",
                        "([ManagerResponse] IS NULL AND [RespondedAt] IS NULL AND [ResponderId] IS NULL) OR " +
                        "([ManagerResponse] IS NOT NULL AND [RespondedAt] IS NOT NULL AND [ResponderId] IS NOT NULL)");
                });
            });


            modelBuilder.Entity<DailyStatistic>(e =>
            {
                e.Property(d => d.Revenue).HasPrecision(10, 2);
                e.HasIndex(d => d.Date).IsUnique();

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_DailyStatistics_Counts", "[ReservationCount] >= 0 AND [GuestCount] >= 0" +
                        " AND [OrderCount] >= 0 AND [NoShowCount] >= 0");
                    t.HasCheckConstraint("CK_DailyStatistics_Revenue", "[Revenue] >= 0");
                    t.HasCheckConstraint("CK_DailyStatistics_NoShow", "[NoShowCount] <= [ReservationCount]");
                });

            });

            modelBuilder.Entity<Prediction>(e =>
            {
                e.Property(p => p.Model).HasConversion<string>().HasMaxLength(30);
                e.Property(p => p.ExpectedGuests).HasPrecision(8, 2);

                e.HasIndex(p => new {p.Date,p.Model}).IsUnique();

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Predictions_ExpectedGuests", "[ExpectedGuests] >= 0");


                });

            });

            modelBuilder.Entity<PinLoginAttempt>(e =>
            {
                e.Property(a => a.ClientKey).HasMaxLength(64);
                e.HasIndex( a => a.ClientKey ).IsUnique();

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_PinLoginAttempts_FailedCount", "[FailedCount] >= 0 ");
                });

            });


        }

       

    } 
    
}

    

   
   
