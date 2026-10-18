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
        var env = provider.GetRequiredService<IHostEnvironment>();
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception) when (env.IsDevelopment() && context.Database.IsSqlite())
        {
            // Local SQLite files from older schemas often have tables but no matching migration history.
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[]
                 {
                     AuthConstants.AdminRole,
                     AuthConstants.StaffRole,
                     AuthConstants.SchoolRole,
                 })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        await MigrateLegacyCompanyRolesAsync(userManager);
        await SeedCatalogAsync(context);
        await EnsureUserSettingsForAllUsersAsync(context);

        var seedEmail = configuration["Seed:CompanyEmail"];
        var seedPassword = configuration["Seed:CompanyPassword"];
        if (string.IsNullOrWhiteSpace(seedEmail) || string.IsNullOrWhiteSpace(seedPassword))
            return;

        if (!seedEmail.EndsWith($"@{AuthConstants.CompanyEmailDomain}", StringComparison.OrdinalIgnoreCase))
            return;

        await SeedCompanyUserIfMissingAsync(userManager, context, seedEmail, seedPassword, isSales: false);

        var salesEmail = configuration["Seed:SalesEmail"];
        var salesPassword = configuration["Seed:SalesPassword"];
        if (!string.IsNullOrWhiteSpace(salesEmail) && !string.IsNullOrWhiteSpace(salesPassword)
            && salesEmail.EndsWith($"@{AuthConstants.CompanyEmailDomain}", StringComparison.OrdinalIgnoreCase))
        {
            await SeedCompanyUserIfMissingAsync(userManager, context, salesEmail, salesPassword, isSales: true);
        }
    }

    private static async Task SeedCompanyUserIfMissingAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
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
            await userManager.AddToRoleAsync(user, AuthConstants.StaffRole);
        }
        else
        {
            await userManager.AddToRoleAsync(user, AuthConstants.AdminRole);
        }

        // Create UserSettings for the user
        await CreateUserSettingsIfMissingAsync(context, user.Id);
    }

    private static async Task CreateUserSettingsIfMissingAsync(ApplicationDbContext context, string userId)
    {
        var existing = await context.UserSettings.AsNoTracking()
            .AnyAsync(s => s.UserId == userId);
        if (existing)
            return;

        context.UserSettings.Add(new UserSettings
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Theme = "auto",
            Language = "en",
            EmailNotificationsEnabled = true,
            LowStockAlertsEnabled = true,
            NewOrderAlertsEnabled = true,
            ItemsPerPage = 20,
            CurrencyCode = "ZAR",
            CompactViewEnabled = false,
            TwoFactorEnabled = false,
            OnboardingCompleted = false,
            LastUpdatedUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
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
                Category = "Stationery",
                StockQuantity = 100,
                ReorderLevel = 99,
                UnitPrice = 24.99m,
            },
            new Product
            {
                Sku = "PEN-BLK",
                Name = "Ballpoint black (box 50)",
                Category = "Stationery",
                StockQuantity = 500,
                ReorderLevel = 40,
                UnitPrice = 189.00m,
            },
            new Product
            {
                Sku = "RUL-30CM",
                Name = "Plastic ruler 30cm",
                Category = "Stationery",
                StockQuantity = 200,
                ReorderLevel = 25,
                UnitPrice = 8.50m,
            },
            // Furniture
            new Product
            {
                Sku = "FURN-CHAIR-STUDENT",
                Name = "Student chair (stackable, metal frame)",
                Category = "Furniture",
                StockQuantity = 250,
                ReorderLevel = 50,
                UnitPrice = 349.00m,
            },
            new Product
            {
                Sku = "FURN-DESK-STANDARD",
                Name = "Student desk (600×450 top)",
                Category = "Furniture",
                StockQuantity = 180,
                ReorderLevel = 40,
                UnitPrice = 599.00m,
            },
            new Product
            {
                Sku = "FURN-DESK-TEACHER",
                Name = "Teacher desk with drawer",
                Category = "Furniture",
                StockQuantity = 40,
                ReorderLevel = 10,
                UnitPrice = 1899.00m,
            },
            new Product
            {
                Sku = "FURN-WHITEBOARD-2M",
                Name = "Magnetic whiteboard 2m",
                Category = "Furniture",
                StockQuantity = 60,
                ReorderLevel = 15,
                UnitPrice = 2299.00m,
            },
            new Product
            {
                Sku = "FURN-WHITEBOARD-MOBILE",
                Name = "Mobile double-sided whiteboard",
                Category = "Furniture",
                StockQuantity = 35,
                ReorderLevel = 8,
                UnitPrice = 2799.00m,
            },
            new Product
            {
                Sku = "FURN-CHALKBOARD-2M",
                Name = "Chalkboard wall-mount 2m",
                Category = "Furniture",
                StockQuantity = 45,
                ReorderLevel = 10,
                UnitPrice = 1699.00m,
            },
            new Product
            {
                Sku = "FURN-LOCKER-3DOOR",
                Name = "Steel locker (3-door column)",
                Category = "Furniture",
                StockQuantity = 55,
                ReorderLevel = 12,
                UnitPrice = 2499.00m,
            },
            new Product
            {
                Sku = "FURN-CABINET-BOOK",
                Name = "Library bookshelf (5-tier)",
                Category = "Furniture",
                StockQuantity = 70,
                ReorderLevel = 20,
                UnitPrice = 1890.00m,
            },
            new Product
            {
                Sku = "FURN-STOOL-LAB",
                Name = "Science lab stool (height adjustable)",
                Category = "Furniture",
                StockQuantity = 90,
                ReorderLevel = 20,
                UnitPrice = 799.00m,
            },
            new Product
            {
                Sku = "FURN-DESK-EXAM",
                Name = "Exam desk (folding)",
                Category = "Furniture",
                StockQuantity = 240,
                ReorderLevel = 60,
                UnitPrice = 429.00m,
            },
            new Product
            {
                Sku = "FURN-NOTICE-BOARD",
                Name = "Cork notice board 1.2m",
                Category = "Furniture",
                StockQuantity = 85,
                ReorderLevel = 20,
                UnitPrice = 599.00m,
            },
            new Product
            {
                Sku = "FURN-PROJECTOR-SCREEN",
                Name = "Projector screen (pull-down, 2m)",
                Category = "Furniture",
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

    /// <summary>Ensures users who only had the legacy Company role also get Admin so the second portal step can resolve.</summary>
    private static async Task MigrateLegacyCompanyRolesAsync(UserManager<ApplicationUser> userManager)
    {
        foreach (var user in userManager.Users.ToList())
        {
            var hasLegacy = await userManager.IsInRoleAsync(user, AuthConstants.CompanyRoleLegacy);
            var hasAdmin = await userManager.IsInRoleAsync(user, AuthConstants.AdminRole);
            var hasSales = await userManager.IsInRoleAsync(user, AuthConstants.StaffRole);
            if (hasLegacy && !hasAdmin && !hasSales)
                await userManager.AddToRoleAsync(user, AuthConstants.AdminRole);
        }
    }

    /// <summary>Ensures all existing users have UserSettings records.</summary>
    private static async Task EnsureUserSettingsForAllUsersAsync(ApplicationDbContext context)
    {
        var usersWithoutSettings = await context.Users
            .Where(u => !context.UserSettings.Any(s => s.UserId == u.Id))
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var userId in usersWithoutSettings)
        {
            context.UserSettings.Add(new UserSettings
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Theme = "auto",
                Language = "en",
                EmailNotificationsEnabled = true,
                LowStockAlertsEnabled = true,
                NewOrderAlertsEnabled = true,
                ItemsPerPage = 20,
                CurrencyCode = "ZAR",
                CompactViewEnabled = false,
                TwoFactorEnabled = false,
                OnboardingCompleted = false,
                LastUpdatedUtc = DateTime.UtcNow
            });
        }

        if (usersWithoutSettings.Count > 0)
            await context.SaveChangesAsync();
    }
}
