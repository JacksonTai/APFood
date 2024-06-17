using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Constants.Order;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APFood.Data;

public class APFoodContext(DbContextOptions<APFoodContext> options) : IdentityDbContext<APFoodUser>(options)
{
    public DbSet<FoodVendor> FoodVendors { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Runner> Runners { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<DeliveryTask> DeliveryTasks { get; set; }
    public DbSet<RunnerDeliveryTask> RunnerDeliveryTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<FoodVendor>()
                   .ToTable("FoodVendors")
                   .HasBaseType<APFoodUser>();

        builder.Entity<Runner>()
           .ToTable("Runners")
           .HasBaseType<APFoodUser>()
           .Property(r => r.Points)
           .HasPrecision(18, 2);

        // Customer
        builder.Entity<Customer>()
               .ToTable("Customers")
               .HasBaseType<APFoodUser>();

        builder.Entity<Customer>()
              .HasOne(c => c.Cart)
              .WithOne(c => c.Customer)
              .HasForeignKey<Customer>(c => c.CartId);

        // Food
        builder.Entity<Food>()
          .Property(f => f.Price)
          .HasPrecision(18, 2);

        // Cart
        builder.Entity<Cart>()
                 .HasMany(c => c.Items)
                 .WithOne(ci => ci.Cart)
                 .HasForeignKey(ci => ci.CartId);

        builder.Entity<CartItem>()
            .HasOne(ci => ci.Food)
            .WithMany()
            .HasForeignKey(ci => ci.FoodId);

        // Order
        builder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);

        builder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);

        builder.Entity<Order>()
            .Property(o => o.QueueNumber)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR QueueNumberSequence");

        builder.Entity<Order>()
            .Property(o => o.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        builder.HasSequence<int>("QueueNumberSequence", schema: "dbo")
            .StartsAt(1)
            .IncrementsBy(1);

        // Order Item
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Food)
            .WithMany()
            .HasForeignKey(oi => oi.FoodId);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        // Payment
        builder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId);

        builder.Entity<Payment>()
            .Property(p => p.Subtotal)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.RunnerPointsUsed)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.DeliveryFee)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.Total)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        // DeliveryTask
        builder.Entity<DeliveryTask>()
           .HasOne(dt => dt.Order)
           .WithOne()
           .HasForeignKey<DeliveryTask>(dt => dt.OrderId);

        builder.Entity<DeliveryTask>()
            .Property(dt => dt.Status)
            .HasConversion<string>()
            .HasDefaultValue(DeliveryStatus.Pending);

        builder.Entity<RunnerDeliveryTask>()
            .HasOne(rdt => rdt.Runner)
            .WithMany()
            .HasForeignKey(rdt => rdt.RunnerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<RunnerDeliveryTask>()
            .HasOne(rdt => rdt.DeliveryTask)
            .WithMany()
            .HasForeignKey(rdt => rdt.DeliveryTaskId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
