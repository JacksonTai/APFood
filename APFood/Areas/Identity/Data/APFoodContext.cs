using APFood.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace APFood.Data;

public class APFoodContext : IdentityDbContext<APFoodUser>
{
    public APFoodContext(DbContextOptions<APFoodContext> options)
        : base(options)
    {
    }

    public DbSet<FoodVendor> FoodVendors { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Runner> Runners { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<FoodVendor>()
                   .ToTable("FoodVendors")
                   .HasBaseType<APFoodUser>();

        builder.Entity<Customer>()
               .ToTable("Customers")
               .HasBaseType<APFoodUser>();

        builder.Entity<Runner>()
               .ToTable("Runners")
               .HasBaseType<APFoodUser>()
               .Property(r => r.Points)
               .HasPrecision(18, 2);

        builder.Entity<Customer>()
              .HasOne(c => c.Cart)
              .WithOne(c => c.Customer)
              .HasForeignKey<Customer>(c => c.CartId);

        builder.Entity<Food>()
          .Property(f => f.Price)
          .HasPrecision(18, 2);

        builder.Entity<Cart>()
                 .HasMany(c => c.Items)
                 .WithOne(ci => ci.Cart)
                 .HasForeignKey(ci => ci.CartId);

        builder.Entity<CartItem>()
            .HasOne(ci => ci.Food)
            .WithMany()
            .HasForeignKey(ci => ci.FoodId);

    }
}
