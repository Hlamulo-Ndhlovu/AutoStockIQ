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

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<Sale> Sales => Set<Sale>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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

        builder.Entity<Supplier>(e =>
        {
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.Email).IsRequired();
            e.Property(s => s.PhoneNumber).IsRequired();
        });

        builder.Entity<Sale>(e =>
        {
            e.Property(s => s.TotalValue).HasPrecision(18, 2);
            e.Property(s => s.SaleNumber).IsRequired();
        });

        builder.Entity<SaleLine>(e =>
        {
            e.Property(l => l.UnitPrice).HasPrecision(18, 2);
            e.Property(l => l.LineTotal).HasPrecision(18, 2);
        });

        builder.Entity<AuditLog>(e =>
        {
            e.Property(a => a.TimestampUtc).IsRequired();
            e.Property(a => a.UserId).IsRequired();
            e.Property(a => a.ActionType).IsRequired();
            e.Property(a => a.EntityType).IsRequired();
            e.Property(a => a.EntityId).IsRequired();
        });
    }
}
