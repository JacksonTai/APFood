using APFood.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APFood.Data;

public class APFoodContext : IdentityDbContext<APFoodUser>
{
    public APFoodContext(DbContextOptions<APFoodContext> options)
        : base(options)
    {
    }

    public DbSet<FoodVendor> FoodVendors { get; set; }


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
               .HasBaseType<APFoodUser>();
    }
}
