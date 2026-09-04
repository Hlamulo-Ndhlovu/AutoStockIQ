using AutoStockIQ.Data;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = AuthConstants.StaffRoles)]
[Route("reports")]
public class ReportsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly PdfGenerationService _pdfService;

    public ReportsController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        PdfGenerationService pdfService)
    {
        _userManager = userManager;
        _db = db;
        _pdfService = pdfService;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        return View();
    }

    [HttpGet("stock")]
    public async Task<IActionResult> StockReport()
    {
        var products = await _db.Products.AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
        var pdfBytes = _pdfService.GenerateStockReportPdf(products);
        return File(pdfBytes, "application/pdf", $"StockReport_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("audit")]
    public async Task<IActionResult> AuditReport(DateTime? startDate, DateTime? endDate)
    {
        var query = _db.AuditLogs.AsNoTracking();
        
        if (startDate.HasValue)
        {
            query = query.Where(log => log.TimestampUtc >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(log => log.TimestampUtc <= endDate.Value.AddDays(1));
        }
        
        var auditLogs = await query
            .OrderByDescending(log => log.TimestampUtc)
            .ToListAsync();
            
        var pdfBytes = _pdfService.GenerateAuditReportPdf(auditLogs, startDate, endDate);
        return File(pdfBytes, "application/pdf", $"AuditReport_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}