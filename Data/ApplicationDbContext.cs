using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<SchoolOrder> SchoolOrders => Set<SchoolOrder>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    public DbSet<StockRefillAlert> StockRefillAlerts => Set<StockRefillAlert>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.UnitPrice).HasPrecision(18, 2);
            e.Property(p => p.Category).IsRequired();
            e.Property(p => p.IsActive).HasDefaultValue(true);
        });

        builder.Entity<OrderLine>(e =>
        {
            e.Property(l => l.UnitPrice).HasPrecision(18, 2);
        });

        builder.Entity<SchoolOrder>(e =>
        {
            e.HasOne(o => o.SchoolUser)
                .WithMany()
                .HasForeignKey(o => o.SchoolUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(o => o.Lines)
                .WithOne(l => l.Order)
                .HasForeignKey(l => l.SchoolOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.Property(o => o.TotalValue).HasPrecision(18, 2);
            e.Property(o => o.OrderNumber).IsRequired();
        });

        builder.Entity<StockRefillAlert>(e =>
        {
            e.HasOne(a => a.Product)
                .WithMany()
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
