using AutoStockIQ.Data;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Authorize]
public class PdfController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PdfGenerationService _pdfService;
    private readonly FirebaseStorageService _storageService;

    public PdfController(
        ApplicationDbContext context,
        PdfGenerationService pdfService,
        FirebaseStorageService storageService)
    {
        _context = context;
        _pdfService = pdfService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> PurchaseOrder(int orderId)
    {
        var order = await _context.SchoolOrders
            .Include(o => o.SchoolUser)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound();
        }

        var productIds = order.Lines.Select(l => l.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var pdfData = _pdfService.GeneratePurchaseOrderPdf(order, order.SchoolUser!, products);
        var pdfUrl = await _storageService.UploadPurchaseOrderPdfAsync(order.OrderNumber, pdfData);

        return Redirect(pdfUrl);
    }

    [HttpGet]
    public async Task<IActionResult> SaleReceipt(string saleId)
    {
        var firestoreService = HttpContext.RequestServices.GetRequiredService<FirestoreService>();
        var sale = await firestoreService.GetSaleAsync(saleId);

        if (sale == null)
        {
            return NotFound();
        }

        var pdfData = _pdfService.GenerateSaleReceiptPdf(sale, sale.Lines.ToList());
        var pdfUrl = await _storageService.UploadSaleReceiptPdfAsync(sale.SaleNumber, pdfData);

        return Redirect(pdfUrl);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> StockReport()
    {
        var products = await _context.Products.ToListAsync();
        var pdfData = _pdfService.GenerateStockReportPdf(products);
        var pdfUrl = await _storageService.UploadStockReportPdfAsync(pdfData);

        return Redirect(pdfUrl);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> AuditReport(DateTime? startDate, DateTime? endDate)
    {
        var auditLogService = HttpContext.RequestServices.GetRequiredService<AuditLogService>();
        var logs = await auditLogService.GetFilteredAuditLogsAsync(startDate, endDate, null, null);
        
        var pdfData = _pdfService.GenerateAuditReportPdf(logs, startDate, endDate);
        var pdfUrl = await _storageService.UploadAuditReportPdfAsync(pdfData, startDate, endDate);

        return Redirect(pdfUrl);
    }
}
