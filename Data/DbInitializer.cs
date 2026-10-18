using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[]
                 {
                     AuthConstants.CompanyRole,
                     AuthConstants.CompanyAdminRole,
                     AuthConstants.CompanySalesRole,
                     AuthConstants.SchoolRole,
                 })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        await MigrateLegacyCompanyRolesAsync(userManager);
        await SeedCatalogAsync(context);

        var seedEmail = configuration["Seed:CompanyEmail"];
        var seedPassword = configuration["Seed:CompanyPassword"];
        if (string.IsNullOrWhiteSpace(seedEmail) || string.IsNullOrWhiteSpace(seedPassword))
            return;

        if (!seedEmail.EndsWith($"@{AuthConstants.CompanyEmailDomain}", StringComparison.OrdinalIgnoreCase))
            return;

        await SeedCompanyUserIfMissingAsync(userManager, seedEmail, seedPassword, isSales: false);

        var salesEmail = configuration["Seed:SalesEmail"];
        var salesPassword = configuration["Seed:SalesPassword"];
        if (!string.IsNullOrWhiteSpace(salesEmail) && !string.IsNullOrWhiteSpace(salesPassword)
            && salesEmail.EndsWith($"@{AuthConstants.CompanyEmailDomain}", StringComparison.OrdinalIgnoreCase))
        {
            await SeedCompanyUserIfMissingAsync(userManager, salesEmail, salesPassword, isSales: true);
        }
    }

    private static async Task SeedCompanyUserIfMissingAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        bool isSales)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return;

        if (isSales)
        {
            await userManager.AddToRoleAsync(user, AuthConstants.CompanySalesRole);
        }
        else
        {
            await userManager.AddToRoleAsync(user, AuthConstants.CompanyAdminRole);
            await userManager.AddToRoleAsync(user, AuthConstants.CompanyRole);
        }
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext context)
    {
        var existingSkusList = await context.Products.AsNoTracking()
            .Select(p => p.Sku)
            .ToListAsync();
        var existingSkus = new HashSet<string>(existingSkusList, StringComparer.OrdinalIgnoreCase);

        var productsToEnsure = new[]
        {
            // Stationery
            new Product
            {
                Sku = "NB-A4-80",
                Name = "A4 exercise book 80pg",
                StockQuantity = 100,
                ReorderLevel = 99,
                UnitPrice = 24.99m,
            },
            new Product
            {
                Sku = "PEN-BLK",
                Name = "Ballpoint black (box 50)",
                StockQuantity = 500,
                ReorderLevel = 40,
                UnitPrice = 189.00m,
            },
            new Product
            {
                Sku = "RUL-30CM",
                Name = "Plastic ruler 30cm",
                StockQuantity = 200,
                ReorderLevel = 25,
                UnitPrice = 8.50m,
            },
            // Furniture
            new Product
            {
                Sku = "FURN-CHAIR-STUDENT",
                Name = "Student chair (stackable, metal frame)",
                StockQuantity = 250,
                ReorderLevel = 50,
                UnitPrice = 349.00m,
            },
            new Product
            {
                Sku = "FURN-DESK-STANDARD",
                Name = "Student desk (600×450 top)",
                StockQuantity = 180,
                ReorderLevel = 40,
                UnitPrice = 599.00m,
            },
            new Product
            {
                Sku = "FURN-DESK-TEACHER",
                Name = "Teacher desk with drawer",
                StockQuantity = 40,
                ReorderLevel = 10,
                UnitPrice = 1899.00m,
            },
            new Product
            {
                Sku = "FURN-WHITEBOARD-2M",
                Name = "Magnetic whiteboard 2m",
                StockQuantity = 60,
                ReorderLevel = 15,
                UnitPrice = 2299.00m,
            },
            new Product
            {
                Sku = "FURN-WHITEBOARD-MOBILE",
                Name = "Mobile double-sided whiteboard",
                StockQuantity = 35,
                ReorderLevel = 8,
                UnitPrice = 2799.00m,
            },
            new Product
            {
                Sku = "FURN-CHALKBOARD-2M",
                Name = "Chalkboard wall-mount 2m",
                StockQuantity = 45,
                ReorderLevel = 10,
                UnitPrice = 1699.00m,
            },
            new Product
            {
                Sku = "FURN-LOCKER-3DOOR",
                Name = "Steel locker (3-door column)",
                StockQuantity = 55,
                ReorderLevel = 12,
                UnitPrice = 2499.00m,
            },
            new Product
            {
                Sku = "FURN-CABINET-BOOK",
                Name = "Library bookshelf (5-tier)",
                StockQuantity = 70,
                ReorderLevel = 20,
                UnitPrice = 1890.00m,
            },
            new Product
            {
                Sku = "FURN-STOOL-LAB",
                Name = "Science lab stool (height adjustable)",
                StockQuantity = 90,
                ReorderLevel = 20,
                UnitPrice = 799.00m,
            },
            new Product
            {
                Sku = "FURN-DESK-EXAM",
                Name = "Exam desk (folding)",
                StockQuantity = 240,
                ReorderLevel = 60,
                UnitPrice = 429.00m,
            },
            new Product
            {
                Sku = "FURN-NOTICE-BOARD",
                Name = "Cork notice board 1.2m",
                StockQuantity = 85,
                ReorderLevel = 20,
                UnitPrice = 599.00m,
            },
            new Product
            {
                Sku = "FURN-PROJECTOR-SCREEN",
                Name = "Projector screen (pull-down, 2m)",
                StockQuantity = 30,
                ReorderLevel = 8,
                UnitPrice = 2199.00m,
            },
        };

        var toAdd = productsToEnsure
            .Where(p => !existingSkus.Contains(p.Sku))
            .ToList();

        if (toAdd.Count == 0)
            return;

        context.Products.AddRange(toAdd);
        await context.SaveChangesAsync();
    }

    /// <summary>Ensures users who only had the legacy Company role also get CompanyAdmin so the second portal step can resolve.</summary>
    private static async Task MigrateLegacyCompanyRolesAsync(UserManager<ApplicationUser> userManager)
    {
        foreach (var user in userManager.Users.ToList())
        {
            var hasLegacy = await userManager.IsInRoleAsync(user, AuthConstants.CompanyRole);
            var hasAdmin = await userManager.IsInRoleAsync(user, AuthConstants.CompanyAdminRole);
            var hasSales = await userManager.IsInRoleAsync(user, AuthConstants.CompanySalesRole);
            if (hasLegacy && !hasAdmin && !hasSales)
                await userManager.AddToRoleAsync(user, AuthConstants.CompanyAdminRole);
        }
    }
}
