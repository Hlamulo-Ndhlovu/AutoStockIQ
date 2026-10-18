using Microsoft.AspNetCore.Identity;
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

    public DbSet<SaleLine> SaleLines => Set<SaleLine>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.SchoolName).HasMaxLength(200);
            e.Property(u => u.BusinessName).HasMaxLength(200);
            e.Property(u => u.PopiaConsentIpAddress).HasMaxLength(64);
        });

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Sku).IsRequired().HasMaxLength(64);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.UnitPrice).HasPrecision(18, 2);
            e.Property(p => p.Category).IsRequired().HasMaxLength(100);
            e.Property(p => p.Location).HasMaxLength(200);
            e.Property(p => p.IsActive).HasDefaultValue(true);
        });

        builder.Entity<OrderLine>(e =>
        {
            e.Ignore(l => l.LineTotal);
            e.Property(l => l.UnitPrice).HasPrecision(18, 2);
            e.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SchoolOrder>(e =>
        {
            e.Ignore(o => o.DecidedAtUtc);
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            e.Property(o => o.ApprovedByUserName).HasMaxLength(256);
            e.Property(o => o.DecisionNote).HasMaxLength(2000);
            e.Property(o => o.TotalValue).HasPrecision(18, 2);

            e.HasOne(o => o.SchoolUser)
                .WithMany()
                .HasForeignKey(o => o.SchoolUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(o => o.Lines)
                .WithOne(l => l.Order)
                .HasForeignKey(l => l.SchoolOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockRefillAlert>(e =>
        {
            e.Property(a => a.Message).HasMaxLength(500);
            e.HasOne(a => a.Product)
                .WithMany()
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Supplier>(e =>
        {
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.ContactPerson).HasMaxLength(200);
            e.Property(s => s.Email).IsRequired().HasMaxLength(256);
            e.Property(s => s.PhoneNumber).IsRequired().HasMaxLength(50);
            e.Property(s => s.Address).HasMaxLength(500);
        });

        builder.Entity<Sale>(e =>
        {
            e.HasIndex(s => s.SaleNumber).IsUnique();
            e.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
            e.Property(s => s.CustomerName).IsRequired().HasMaxLength(200);
            e.Property(s => s.CustomerType).IsRequired().HasMaxLength(50);
            e.Property(s => s.ProcessedByUserName).HasMaxLength(256);
            e.Property(s => s.TotalValue).HasPrecision(18, 2);

            e.HasMany(s => s.Lines)
                .WithOne(l => l.Sale)
                .HasForeignKey(l => l.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SaleLine>(e =>
        {
            e.Property(l => l.ProductSku).HasMaxLength(64);
            e.Property(l => l.ProductName).HasMaxLength(200);
            e.Property(l => l.UnitPrice).HasPrecision(18, 2);
            e.Property(l => l.LineTotal).HasPrecision(18, 2);
            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AuditLog>(e =>
        {
            e.Property(a => a.TimestampUtc).IsRequired();
            e.Property(a => a.UserId).IsRequired();
            e.Property(a => a.UserName).HasMaxLength(256);
            e.Property(a => a.UserEmail).HasMaxLength(256);
            e.Property(a => a.IpAddress).HasMaxLength(64);
            e.Property(a => a.ActionType).IsRequired().HasMaxLength(100);
            e.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            e.Property(a => a.EntityId).IsRequired();
            e.Property(a => a.Description).HasMaxLength(1000);
        });

        builder.Entity<UserSettings>(e =>
        {
            e.HasIndex(u => u.UserId).IsUnique();
            e.Property(u => u.Theme).HasMaxLength(20);
            e.Property(u => u.Language).HasMaxLength(10);
            e.Property(u => u.CurrencyCode).HasMaxLength(10);
            e.Property(u => u.TwoFactorPhoneNumber).HasMaxLength(50);

            e.HasOne(u => u.User)
                .WithOne()
                .HasForeignKey<UserSettings>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
