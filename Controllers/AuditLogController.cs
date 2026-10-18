using AutoStockIQ.Data;
using AutoStockIQ.Models.ViewModels;
using AutoStockIQ.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoStockIQ.Controllers;

[Authorize(Roles = AuthConstants.StaffRoles)]
[Route("audit")]
public class AuditLogController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public AuditLogController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(AuditLogFilterViewModel filter)
    {
        ViewData["BodyClass"] = "app-shell app-shell--company";
        
        var query = _db.AuditLogs.AsNoTracking();
        
        // Apply filters
        if (!string.IsNullOrEmpty(filter.UserId))
        {
            query = query.Where(log => log.UserId == filter.UserId);
        }
        
        if (!string.IsNullOrEmpty(filter.ActionType))
        {
            query = query.Where(log => log.ActionType == filter.ActionType);
        }
        
        if (!string.IsNullOrEmpty(filter.EntityType))
        {
            query = query.Where(log => log.EntityType == filter.EntityType);
        }
        
        if (filter.StartDate.HasValue)
        {
            query = query.Where(log => log.TimestampUtc >= filter.StartDate.Value);
        }
        
        if (filter.EndDate.HasValue)
        {
            query = query.Where(log => log.TimestampUtc <= filter.EndDate.Value.AddDays(1));
        }
        
        var auditLogs = await query
            .OrderByDescending(log => log.TimestampUtc)
            .Take(100)
            .ToListAsync();
            
        // Get available filter options
        ViewBag.Users = await _userManager.Users.Select(u => new { u.Id, u.Email }).ToListAsync();
        ViewBag.ActionTypes = await _db.AuditLogs.Select(log => log.ActionType).Distinct().ToListAsync();
        ViewBag.EntityTypes = await _db.AuditLogs.Select(log => log.EntityType).Distinct().ToListAsync();
        
        return View(new AuditLogIndexViewModel
        {
            Filter = filter,
            AuditLogs = auditLogs
        });
    }
}