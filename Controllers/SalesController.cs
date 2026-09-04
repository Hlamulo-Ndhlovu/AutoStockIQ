using AutoStockIQ.Data;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class SalesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly FirestoreService _firestoreService;
    private readonly AuditLogService _auditLogService;
    private readonly PdfGenerationService _pdfService;
    private readonly FirebaseStorageService _storageService;

    public SalesController(
        ApplicationDbContext context,
        FirestoreService firestoreService,
        AuditLogService auditLogService,
        PdfGenerationService pdfService,
        FirebaseStorageService storageService)
    {
        _context = context;
        _firestoreService = firestoreService;
        _auditLogService = auditLogService;
        _pdfService = pdfService;
        _storageService = storageService;
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new SaleCreateViewModel
        {
            Products = _context.Products.Where(p => p.IsActive && p.StockQuantity > 0).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaleCreateViewModel model)
    {
        if (!ModelState.IsValid || model.SaleLines == null || !model.SaleLines.Any())
        {
            model.Products = _context.Products.Where(p => p.IsActive && p.StockQuantity > 0).ToList();
            return View(model);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.Identity?.Name ?? "";
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";

        // Generate sale number
        var saleNumber = $"SL-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var sale = new Sale
        {
            SaleNumber = saleNumber,
            CustomerId = model.CustomerId,
            CustomerName = model.CustomerName,
            CustomerType = model.CustomerType,
            ProcessedByUserId = userId,
            ProcessedByUserName = userName,
            CreatedAtUtc = DateTime.UtcNow
        };

        var saleLines = new List<SaleLine>();
        decimal totalValue = 0;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var line in model.SaleLines.Where(l => l.Quantity > 0))
            {
                var product = await _context.Products.FindAsync(line.ProductId);
                if (product == null || product.StockQuantity < line.Quantity)
                {
                    ModelState.AddModelError("", $"Insufficient stock for {product?.Name ?? "product"}");
                    model.Products = _context.Products.Where(p => p.IsActive && p.StockQuantity > 0).ToList();
                    return View(model);
                }

                // Deduct stock
                product.StockQuantity -= line.Quantity;

                var saleLine = new SaleLine
                {
                    ProductId = product.Id,
                    ProductSku = product.Sku,
                    ProductName = product.Name,
                    Quantity = line.Quantity,
                    UnitPrice = product.UnitPrice,
                    LineTotal = line.Quantity * product.UnitPrice
                };

                saleLines.Add(saleLine);
                totalValue += saleLine.LineTotal;

                // Log stock movement
                await _auditLogService.LogStockMovementAsync(
                    userId, userName, userEmail,
                    product.Id, product.Sku, -line.Quantity,
                    $"Sale {saleNumber}"
                );
            }

            sale.TotalValue = totalValue;
            sale.Lines = saleLines;

            // Save sale to Firestore
            var saleId = await _firestoreService.CreateSaleAsync(sale);

            // Save stock changes to SQL
            await _context.SaveChangesAsync();

            // Log sale
            await _auditLogService.LogSaleAsync(userId, userName, userEmail, saleNumber, totalValue);

            await transaction.CommitAsync();

            // Generate PDF receipt
            var pdfData = _pdfService.GenerateSaleReceiptPdf(sale, saleLines);
            var pdfUrl = await _storageService.UploadSaleReceiptPdfAsync(saleNumber, pdfData);

            TempData["Success"] = $"Sale {saleNumber} completed successfully. Receipt: {pdfUrl}";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "An error occurred while processing the sale.");
            model.Products = _context.Products.Where(p => p.IsActive && p.StockQuantity > 0).ToList();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sales = await _firestoreService.GetAllSalesAsync();
        return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var sale = await _firestoreService.GetSaleAsync(id);
        if (sale == null)
        {
            return NotFound();
        }
        return View(sale);
    }
}

public class SaleCreateViewModel
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = "School"; // School or Business
    public List<SaleLineItem> SaleLines { get; set; } = new();
    public List<Product> Products { get; set; } = new();
}

public class SaleLineItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
