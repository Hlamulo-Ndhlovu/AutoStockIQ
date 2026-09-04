using AutoStockIQ.Data;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = "Admin")]
public class AuditLogController : Controller
{
    private readonly AuditLogService _auditLogService;

    public AuditLogController(AuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? userId, string? actionType)
    {
        var logs = await _auditLogService.GetFilteredAuditLogsAsync(startDate, endDate, userId, actionType);
        
        var viewModel = new AuditLogFilterViewModel
        {
            AuditLogs = logs,
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            ActionType = actionType
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(DateTime? startDate, DateTime? endDate, string? userId, string? actionType)
    {
        var logs = await _auditLogService.GetFilteredAuditLogsAsync(startDate, endDate, userId, actionType);
        
        var pdfService = HttpContext.RequestServices.GetRequiredService<PdfGenerationService>();
        var storageService = HttpContext.RequestServices.GetRequiredService<FirebaseStorageService>();
        
        var pdfData = pdfService.GenerateAuditReportPdf(logs, startDate, endDate);
        var pdfUrl = await storageService.UploadAuditReportPdfAsync(pdfData, startDate, endDate);
        
        return Redirect(pdfUrl);
    }
}

public class AuditLogFilterViewModel
{
    public List<AuditLog> AuditLogs { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UserId { get; set; }
    public string? ActionType { get; set; }
}
