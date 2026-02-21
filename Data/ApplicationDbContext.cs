using GroceryStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.IsAdmin).HasDefaultValue(false);
                entity.Property(u => u.IsActive).HasDefaultValue(true);
            });

            // Category configurations
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // Product configurations
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.PriceCents).HasDefaultValue(0);
                entity.Property(p => p.Stock).HasDefaultValue(0);
                entity.Property(p => p.IsActive).HasDefaultValue(true);
            });

            // Cart configurations
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithOne(u => u.Cart)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItem configurations
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.Property(ci => ci.Quantity).HasDefaultValue(1);
            });

            // Order configurations
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.Property(o => o.Status).HasDefaultValue("Pending");
            });

            // Delivery configurations
            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.Property(d => d.Status).HasDefaultValue("Scheduled");
            });

            // Payment configurations
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Status).HasDefaultValue("Pending");
            });

            // Notification configurations
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.IsRead).HasDefaultValue(false);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fruits", Slug = "fruits" },
                new Category { Id = 2, Name = "Vegetables", Slug = "vegetables" },
                new Category { Id = 3, Name = "Dairy", Slug = "dairy" },
                new Category { Id = 4, Name = "Bakery", Slug = "bakery" },
                new Category { Id = 5, Name = "Beverages", Slug = "beverages" },
                new Category { Id = 6, Name = "Snacks", Slug = "snacks" },
                new Category { Id = 7, Name = "Household", Slug = "household" }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, CategoryId = 1, Name = "Fresh Apples", Unit = "kg", Description = "Crisp and juicy red apples", PriceCents = 12000, Stock = 100, ImageUrl = "/images/products/apples.jpg" },
                new Product { Id = 2, CategoryId = 1, Name = "Bananas", Unit = "dozen", Description = "Ripe yellow bananas", PriceCents = 5000, Stock = 150, ImageUrl = "/images/products/bananas.jpg" },
                new Product { Id = 3, CategoryId = 1, Name = "Oranges", Unit = "kg", Description = "Sweet and tangy oranges", PriceCents = 8000, Stock = 80, ImageUrl = "/images/products/oranges.jpg" },
                new Product { Id = 4, CategoryId = 2, Name = "Tomatoes", Unit = "kg", Description = "Fresh red tomatoes", PriceCents = 4000, Stock = 200, ImageUrl = "/images/products/tomatoes.jpg" },
                new Product { Id = 5, CategoryId = 2, Name = "Potatoes", Unit = "kg", Description = "Premium quality potatoes", PriceCents = 3000, Stock = 300, ImageUrl = "/images/products/potatoes.jpg" },
                new Product { Id = 6, CategoryId = 2, Name = "Onions", Unit = "kg", Description = "Fresh onions", PriceCents = 3500, Stock = 250, ImageUrl = "/images/products/onions.jpg" },
                new Product { Id = 7, CategoryId = 3, Name = "Fresh Milk", Unit = "liter", Description = "Full cream milk", PriceCents = 6000, Stock = 100, ImageUrl = "/images/products/milk.jpg" },
                new Product { Id = 8, CategoryId = 3, Name = "Cheese", Unit = "200g", Description = "Cheddar cheese block", PriceCents = 15000, Stock = 50, ImageUrl = "/images/products/cheese.jpg" },
                new Product { Id = 9, CategoryId = 3, Name = "Butter", Unit = "500g", Description = "Salted butter", PriceCents = 25000, Stock = 60, ImageUrl = "/images/products/butter.jpg" },
                new Product { Id = 10, CategoryId = 4, Name = "White Bread", Unit = "400g", Description = "Soft white bread", PriceCents = 3500, Stock = 80, ImageUrl = "/images/products/bread.jpg" },
                new Product { Id = 11, CategoryId = 4, Name = "Croissants", Unit = "4 pcs", Description = "Buttery croissants", PriceCents = 12000, Stock = 40, ImageUrl = "/images/products/croissants.jpg" },
                new Product { Id = 12, CategoryId = 5, Name = "Orange Juice", Unit = "1L", Description = "100% pure orange juice", PriceCents = 8500, Stock = 70, ImageUrl = "/images/products/orange-juice.jpg" },
                new Product { Id = 13, CategoryId = 5, Name = "Mineral Water", Unit = "1L", Description = "Pure mineral water", PriceCents = 2000, Stock = 500, ImageUrl = "/images/products/water.jpg" },
                new Product { Id = 14, CategoryId = 6, Name = "Potato Chips", Unit = "150g", Description = "Crispy potato chips", PriceCents = 3000, Stock = 120, ImageUrl = "/images/products/chips.jpg" },
                new Product { Id = 15, CategoryId = 6, Name = "Chocolate Bar", Unit = "100g", Description = "Milk chocolate", PriceCents = 5000, Stock = 100, ImageUrl = "/images/products/chocolate.jpg" },
                new Product { Id = 16, CategoryId = 7, Name = "Dish Soap", Unit = "500ml", Description = "Lemon dish soap", PriceCents = 4500, Stock = 80, ImageUrl = "/images/products/dish-soap.jpg" },
                new Product { Id = 17, CategoryId = 7, Name = "Laundry Detergent", Unit = "1kg", Description = "Powder detergent", PriceCents = 18000, Stock = 60, ImageUrl = "/images/products/detergent.jpg" }
            );
        }
    }
}
