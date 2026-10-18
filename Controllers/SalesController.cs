using AutoStockIQ.Data;
using AutoStockIQ.Models.ViewModels;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = AuthConstants.StaffRoles)]
[Route("sales")]
public class SalesController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly PdfGenerationService _pdfService;

    public SalesController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        PdfGenerationService pdfService)
    {
        _userManager = userManager;
        _db = db;
        _pdfService = pdfService;
    }

    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        ViewBag.TotalSales = await _db.Sales.AsNoTracking().CountAsync();
        var sales = await _db.Sales.AsNoTracking().ToListAsync();
        ViewBag.TotalValue = sales.Sum(s => s.TotalValue);
        ViewBag.RecentSales = await _db.Sales.AsNoTracking()
            .Include(s => s.Lines)
            .OrderByDescending(s => s.CreatedAtUtc)
            .Take(10)
            .ToListAsync();

        ViewData["BodyClass"] = "app-shell app-shell--company";
        return View();
    }

    [HttpGet("new")]
    public async Task<IActionResult> NewSale()
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View(products);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSale(CreateSaleViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(NewSale));

        // Generate sale number
        var saleCount = await _db.Sales.CountAsync() + 1;
        var saleNumber = $"SL-{saleCount:D4}";

        // Calculate total and create sale lines
        var lines = new List<SaleLine>();
        decimal totalValue = 0;

        foreach (var item in model.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product is null || product.StockQuantity < item.Quantity)
            {
                TempData["SaleError"] = $"Insufficient stock for {product?.Name ?? "product"}";
                return RedirectToAction(nameof(NewSale));
            }

            var lineTotal = item.Quantity * product.UnitPrice;
            totalValue += lineTotal;

            lines.Add(new SaleLine
            {
                Id = Guid.NewGuid().ToString(),
                SaleId = saleNumber,
                ProductId = product.Id,
                ProductSku = product.Sku,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.UnitPrice,
                LineTotal = lineTotal
            });

            // Update stock
            product.StockQuantity -= item.Quantity;
        }

        // Create sale
        var sale = new Sale
        {
            Id = Guid.NewGuid().ToString(),
            SaleNumber = saleNumber,
            CustomerName = model.CustomerName,
            CustomerType = model.CustomerType,
            CreatedAtUtc = DateTime.UtcNow,
            TotalValue = totalValue,
            ProcessedByUserId = user.Id,
            ProcessedByUserName = user.Email ?? "Unknown",
            Lines = lines
        };

        await _db.Sales.AddAsync(sale);
        await _db.SaveChangesAsync();

        TempData["SaleSuccess"] = $"Sale {saleNumber} created successfully. Total: R{totalValue:F2}";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        var sales = await _db.Sales.AsNoTracking()
            .Include(s => s.Lines)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync();
        return View(sales);
    }

    [HttpGet("receipt/{id}")]
    public async Task<IActionResult> Receipt(string id)
    {
        var sale = await _db.Sales.AsNoTracking()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null)
            return NotFound();

        var pdfBytes = await _pdfService.GenerateSaleReceiptAsync(sale);
        return File(pdfBytes, "application/pdf", $"Receipt_{sale.SaleNumber}.pdf");
    }
}