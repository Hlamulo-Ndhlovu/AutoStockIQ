using AutoStockIQ.Data;
using AutoStockIQ.Models.ViewModels;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = AuthConstants.StaffRoles)]
[Route("products")]
public class ProductController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly AuditLogService _auditLog;

    public ProductController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        AuditLogService auditLog)
    {
        _userManager = userManager;
        _db = db;
        _auditLog = auditLog;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var products = await _db.Products.AsNoTracking()
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
        return View(products);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        await LoadSuppliersAsync();
        return View(new ProductViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        if (await _db.Products.AnyAsync(p => p.Sku == model.Sku))
            ModelState.AddModelError(nameof(model.Sku), "A product with this SKU already exists.");

        if (!ModelState.IsValid)
        {
            await LoadSuppliersAsync();
            return View(model);
        }

        var product = new Product
        {
            Sku = model.Sku.Trim(),
            Name = model.Name.Trim(),
            Category = string.IsNullOrWhiteSpace(model.Category) ? "Stationery" : model.Category.Trim(),
            StockQuantity = model.StockQuantity,
            ReorderLevel = model.ReorderLevel,
            UnitPrice = model.UnitPrice,
            SupplierId = string.IsNullOrWhiteSpace(model.SupplierId) ? null : model.SupplierId,
            Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim(),
            IsActive = model.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "CREATE", "Product", product.Id.ToString(),
            $"Created product {product.Sku}: {product.Name}");

        TempData["ProductSuccess"] = $"Product '{product.Name}' added to the catalogue.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        await LoadSuppliersAsync();
        return View(new ProductViewModel
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Category = product.Category,
            StockQuantity = product.StockQuantity,
            ReorderLevel = product.ReorderLevel,
            UnitPrice = product.UnitPrice,
            SupplierId = product.SupplierId,
            Location = product.Location,
            IsActive = product.IsActive
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel model)
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        if (await _db.Products.AnyAsync(p => p.Sku == model.Sku && p.Id != id))
            ModelState.AddModelError(nameof(model.Sku), "A product with this SKU already exists.");

        if (!ModelState.IsValid)
        {
            await LoadSuppliersAsync();
            return View(model);
        }

        product.Sku = model.Sku.Trim();
        product.Name = model.Name.Trim();
        product.Category = string.IsNullOrWhiteSpace(model.Category) ? "Stationery" : model.Category.Trim();
        product.StockQuantity = model.StockQuantity;
        product.ReorderLevel = model.ReorderLevel;
        product.UnitPrice = model.UnitPrice;
        product.SupplierId = string.IsNullOrWhiteSpace(model.SupplierId) ? null : model.SupplierId;
        product.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim();
        product.IsActive = model.IsActive;
        product.DeactivatedAtUtc = model.IsActive ? null : product.DeactivatedAtUtc ?? DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "UPDATE", "Product", product.Id.ToString(),
            $"Updated product {product.Sku}: {product.Name}");

        TempData["ProductSuccess"] = $"Product '{product.Name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("deactivate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        product.IsActive = false;
        product.DeactivatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "DEACTIVATE", "Product", product.Id.ToString(),
            $"Deactivated product {product.Sku}");

        TempData["ProductSuccess"] = $"Product '{product.Name}' deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("activate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        product.IsActive = true;
        product.DeactivatedAtUtc = null;
        await _db.SaveChangesAsync();

        await _auditLog.LogAsync(user.Id, user.UserName ?? "Unknown", user.Email ?? "unknown", "ACTIVATE", "Product", product.Id.ToString(),
            $"Activated product {product.Sku}");

        TempData["ProductSuccess"] = $"Product '{product.Name}' activated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSuppliersAsync()
    {
        ViewBag.Suppliers = await _db.Suppliers.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}
